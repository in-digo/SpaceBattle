using System.Collections.Concurrent;
using Moq;

namespace SpaceBattle.Lib.Tests;

public class CommandRedirectionTests
{
    // Команда переносится из исходной очереди в целевую и выполняется потоком второго процессора
    [Fact]
    public void Processors_RedirectCommandAndExecuteItInTargetThread()
    {
        using var sourceQueue = new BlockingCollection<ICommand>();
        using var targetQueue = new BlockingCollection<ICommand>();
        using var targetCommandExecuted = new ManualResetEventSlim();

        var sourceContext = new Dictionary<string, object>
        {
            ["canContinue"] = true
        };

        var targetContext = new Dictionary<string, object>
        {
            ["canContinue"] = true
        };

        var sourceThreadId = 0;
        var executionThreadId = 0;

        var sourceThreadMarker = new Mock<ICommand>(MockBehavior.Strict);
        sourceThreadMarker
            .Setup(command => command.Execute())
            .Callback(() => sourceThreadId = Environment.CurrentManagedThreadId);

        var redirectedCommand = new Mock<ICommand>(MockBehavior.Strict);
        redirectedCommand
            .Setup(command => command.Execute())
            .Callback(() =>
            {
                executionThreadId = Environment.CurrentManagedThreadId;
                targetCommandExecuted.Set();
            });

        var sourceNormalState = new NormalState(sourceQueue);
        var targetNormalState = new NormalState(targetQueue);

        var moveToState = new MoveToState(sourceQueue, targetQueue);

        sourceQueue.Add(sourceThreadMarker.Object);
        sourceQueue.Add(new MoveToCommand(moveToState));
        sourceQueue.Add(redirectedCommand.Object);
        sourceQueue.Add(new RunCommand(sourceNormalState));
        sourceQueue.Add(new HardStopCommand(sourceContext));

        Exception? sourceTerminationException = null;
        Exception? targetTerminationException = null;

        // Запускаем целевой процессор: он будет ждать команду в targetQueue
        var targetProcessor = new Processor(
            new StateMachineProcessable(
                targetNormalState,
                exception => targetTerminationException = exception));

        var sourceProcessor = new Processor(
            new StateMachineProcessable(
                sourceNormalState,
                exception => sourceTerminationException = exception));

        var wasExecuted =
            targetCommandExecuted.Wait(5000);

        // Целевой процессор останавливаем после того, как он обработал перенаправленную команду
        targetQueue.Add(new HardStopCommand(targetContext));

        Assert.True(sourceProcessor.Wait(5000));
        Assert.True(targetProcessor.Wait(5000));

        Assert.True(wasExecuted);
        Assert.NotEqual(0, sourceThreadId);
        Assert.NotEqual(0, executionThreadId);
        Assert.NotEqual(sourceThreadId, executionThreadId);

        sourceThreadMarker.Verify(
            command => command.Execute(),
            Times.Once());

        redirectedCommand.Verify(
            command => command.Execute(),
            Times.Once());

        Assert.Null(sourceTerminationException);
        Assert.Null(targetTerminationException);
    }
}