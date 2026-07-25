using System.Collections.Concurrent;
using Moq;

namespace SpaceBattle.Lib.Tests;

public class NormalStateTests
{
    // Обычное состояние извлекает и выполняет одну команду, после чего остаётся текущим состоянием
    [Fact]
    public void Handle_ExecutesNextCommand_AndRemainsInCurrentState()
    {
        var command = new Mock<ICommand>(MockBehavior.Strict);
        command.Setup(cmd => cmd.Execute());

        using var queue = new BlockingCollection<ICommand>();
        queue.Add(command.Object);
        queue.CompleteAdding();

        var state = new NormalState(queue);

        var nextState = state.Handle();

        Assert.Same(state, nextState);
        command.Verify(cmd => cmd.Execute(), Times.Once());
    }

    // MoveToCommand переводит процессор из обычного режима в заранее созданное состояние перенаправления
    [Fact]
    public void Handle_MoveToCommand_SwitchesToMoveToState()
    {
        using var sourceQueue = new BlockingCollection<ICommand>();
        using var targetQueue = new BlockingCollection<ICommand>();

        var moveToState = new MoveToState(
            sourceQueue,
            targetQueue);

        sourceQueue.Add(new MoveToCommand(moveToState));
        sourceQueue.CompleteAdding();

        var normalState = new NormalState(sourceQueue);

        var nextState = normalState.Handle();

        Assert.Same(moveToState, nextState);
        Assert.False(targetQueue.TryTake(out _));
    }
}