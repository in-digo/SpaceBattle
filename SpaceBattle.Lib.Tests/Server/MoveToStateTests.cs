using System.Collections.Concurrent;
using Moq;

namespace SpaceBattle.Lib.Tests;

public class MoveToStateTests
{
    // Состояние MoveTo извлекает одну команду и переносит её в целевую очередь без выполнения в исходном потоке
    [Fact]
    public void Handle_RedirectsNextCommand_WithoutExecutingItInSourceState()
    {
        var command = new Mock<ICommand>(MockBehavior.Strict);

        using var sourceQueue = new BlockingCollection<ICommand>();
        using var targetQueue = new BlockingCollection<ICommand>();

        sourceQueue.Add(command.Object);
        sourceQueue.CompleteAdding();

        var state = new MoveToState(sourceQueue, targetQueue);

        var nextState = state.Handle();

        Assert.Same(state, nextState);

        Assert.True(targetQueue.TryTake(out var redirectedCommand));
        Assert.Same(command.Object, redirectedCommand);

        command.Verify(
            cmd => cmd.Execute(),
            Times.Never());
    }

    // Команда перехода выполняется в исходном состоянии, не перенаправляется и определяет следующее состояние автомата
    [Fact]
    public void Handle_ExecutesTransitionCommandAndReturnsItsNextStateWithoutRedirecting()
    {
        var nextState = new Mock<ICommandProcessingState>(MockBehavior.Strict);

        var transitionCommand = new Mock<IStateTransitionCommand>(MockBehavior.Strict);

        transitionCommand
            .Setup(command => command.Execute());

        transitionCommand
            .SetupGet(command => command.NextState)
            .Returns(nextState.Object);

        using var sourceQueue = new BlockingCollection<ICommand>();
        using var targetQueue = new BlockingCollection<ICommand>();

        sourceQueue.Add(transitionCommand.Object);
        sourceQueue.CompleteAdding();

        var state = new MoveToState(sourceQueue, targetQueue);

        var actualNextState = state.Handle();

        Assert.Same(nextState.Object, actualNextState);
        Assert.False(targetQueue.TryTake(out _));

        transitionCommand.Verify(
            command => command.Execute(),
            Times.Once());

        transitionCommand.VerifyGet(
            command => command.NextState,
            Times.Once());
    }

    // RunCommand возвращает процессор из режима перенаправления в обычный режим обработки исходной очереди
    [Fact]
    public void Handle_RunCommand_SwitchesBackToNormalState()
    {
        using var sourceQueue = new BlockingCollection<ICommand>();
        using var targetQueue = new BlockingCollection<ICommand>();

        var normalState = new NormalState(sourceQueue);

        sourceQueue.Add(new RunCommand(normalState));
        sourceQueue.CompleteAdding();

        var moveToState = new MoveToState(
            sourceQueue,
            targetQueue);

        var nextState = moveToState.Handle();

        Assert.Same(normalState, nextState);
        Assert.False(targetQueue.TryTake(out _));
    }

    // HardStopCommand останавливает обработку даже в режиме перенаправления и не попадает в целевую очередь
    [Fact]
    public void Handle_HardStopCommand_StopsProcessingWithoutRedirecting()
    {
        var context = new Dictionary<string, object>
        {
            ["canContinue"] = true
        };

        using var sourceQueue = new BlockingCollection<ICommand>();
        using var targetQueue = new BlockingCollection<ICommand>();

        sourceQueue.Add(new HardStopCommand(context));
        sourceQueue.CompleteAdding();

        var state = new MoveToState(
            sourceQueue,
            targetQueue);

        var nextState = state.Handle();

        Assert.Null(nextState);
        Assert.False((bool)context["canContinue"]);
        Assert.False(targetQueue.TryTake(out _));
    }
}