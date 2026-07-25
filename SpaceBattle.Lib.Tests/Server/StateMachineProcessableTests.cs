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
}