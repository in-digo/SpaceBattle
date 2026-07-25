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
}