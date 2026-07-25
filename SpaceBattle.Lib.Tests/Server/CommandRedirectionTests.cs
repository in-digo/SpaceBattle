using System.Collections.Concurrent;
using System.Threading;
using Moq;

namespace SpaceBattle.Lib.Tests;

public class CommandRedirectionTests
{
    // MoveTo переносит команду в целевой поток, а Run возвращает последующую обработку в исходный поток
    [Fact]
    public void Processors_RedirectCommandAndReturnExecutionToSourceThread()
    {
        using var sourceQueue = new BlockingCollection<ICommand>();
        using var targetQueue = new BlockingCollection<ICommand>();
        using var redirectedCommandExecuted = new ManualResetEventSlim();
        using var commandAfterRunExecuted = new ManualResetEventSlim();

        var sourceContext = new Dictionary<string, object>
        {
            ["canContinue"] = true
        };

        var targetContext = new Dictionary<string, object>
        {
            ["canContinue"] = true
        };

        var sourceThreadId = 0;
        var redirectedCommandThreadId = 0;
        var commandAfterRunThreadId = 0;

        var sourceThreadMarker = new Mock<ICommand>(MockBehavior.Strict);
        sourceThreadMarker
            .Setup(command => command.Execute())
            .Callback(() => sourceThreadId = Environment.CurrentManagedThreadId);

        var redirectedCommand = new Mock<ICommand>(MockBehavior.Strict);
        redirectedCommand
            .Setup(command => command.Execute())
            .Callback(() =>
            {
                redirectedCommandThreadId = Environment.CurrentManagedThreadId;
                redirectedCommandExecuted.Set();
            });

        var commandAfterRun = new Mock<ICommand>(MockBehavior.Strict);
        commandAfterRun
            .Setup(command => command.Execute())
            .Callback(() =>
            {
                commandAfterRunThreadId = Environment.CurrentManagedThreadId;
                commandAfterRunExecuted.Set();
            });

        var sourceNormalState = new NormalState(sourceQueue);
        var targetNormalState = new NormalState(targetQueue);

        var moveToState = new MoveToState(sourceQueue, targetQueue);

        sourceQueue.Add(sourceThreadMarker.Object);
        sourceQueue.Add(new MoveToCommand(moveToState));
        sourceQueue.Add(redirectedCommand.Object);
        sourceQueue.Add(new RunCommand(sourceNormalState));
        sourceQueue.Add(commandAfterRun.Object);
        sourceQueue.Add(new HardStopCommand(sourceContext));

        Exception? sourceTerminationException = null;
        Exception? targetTerminationException = null;

        var targetProcessor = new Processor(
            new StateMachineProcessable(
                targetNormalState,
                exception => targetTerminationException = exception));

        var sourceProcessor = new Processor(
            new StateMachineProcessable(
                sourceNormalState,
                exception => sourceTerminationException = exception));

        var wasRedirectedCommandExecuted = redirectedCommandExecuted.Wait(5000);
        var wasCommandAfterRunExecuted = commandAfterRunExecuted.Wait(5000);

        targetQueue.Add(new HardStopCommand(targetContext));

        Assert.True(sourceProcessor.Wait(5000));
        Assert.True(targetProcessor.Wait(5000));

        Assert.True(wasRedirectedCommandExecuted);
        Assert.True(wasCommandAfterRunExecuted);

        Assert.NotEqual(0, sourceThreadId);
        Assert.NotEqual(0, redirectedCommandThreadId);
        Assert.NotEqual(0, commandAfterRunThreadId);

        Assert.NotEqual(
            sourceThreadId,
            redirectedCommandThreadId);

        Assert.Equal(
            sourceThreadId,
            commandAfterRunThreadId);

        sourceThreadMarker.Verify(
            command => command.Execute(),
            Times.Once());

        redirectedCommand.Verify(
            command => command.Execute(),
            Times.Once());

        commandAfterRun.Verify(
            command => command.Execute(),
            Times.Once());

        Assert.Null(sourceTerminationException);
        Assert.Null(targetTerminationException);
    }
}