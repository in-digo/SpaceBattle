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
}