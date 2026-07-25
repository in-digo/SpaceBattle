using System.Collections.Concurrent;
using Moq;

namespace SpaceBattle.Lib.Tests;

public class StateMachineProcessableTests
{
    // Используется состояние, возвращённое предыдущим состоянием
    [Fact]
    public void StateMachineProcessable_UsesStateReturnedByCurrentState()
    {
        var firstState = new Mock<ICommandProcessingState>(MockBehavior.Strict);
        var secondState = new Mock<ICommandProcessingState>(MockBehavior.Strict);

        firstState
            .Setup(state => state.Handle())
            .Returns(secondState.Object);

        secondState
            .Setup(state => state.Handle())
            .Returns((ICommandProcessingState?)null);

        var processable = new StateMachineProcessable(
            firstState.Object,
            _ => { });

        Assert.True(processable.CanContinue);

        processable.Process();

        Assert.True(processable.CanContinue);

        processable.Process();

        Assert.False(processable.CanContinue);

        firstState.Verify(state => state.Handle(), Times.Once());
        secondState.Verify(state => state.Handle(), Times.Once());
    }

    // После HardStopCommand поток завершается, а следующая команда не выполняется
    [Fact]
    public void Processor_StopsAfterHardStopCommand()
    {
        var context = new Dictionary<string, object>
        {
            ["canContinue"] = true
        };

        using var queue = new BlockingCollection<ICommand>();

        var afterStop = new Mock<ICommand>(MockBehavior.Strict);
        afterStop.Setup(command => command.Execute());

        queue.Add(new HardStopCommand(context));
        queue.Add(afterStop.Object);
        queue.CompleteAdding();

        Exception? terminationException = null;

        var state = new NormalState(queue);
        var processable = new StateMachineProcessable(
            state,
            exception => terminationException = exception);

        var processor = new Processor(processable);

        Assert.True(processor.Wait(5000));

        afterStop.Verify(
            command => command.Execute(),
            Times.Never());

        Assert.Null(terminationException);
    }
}