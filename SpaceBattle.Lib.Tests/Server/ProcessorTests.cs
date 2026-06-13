using System.Collections.Concurrent;
using Moq;

namespace SpaceBattle.Lib.Tests;

public class ProcessorTests
{
    // Поток в цикле берёт команды из очереди и выполняет их
    [Fact]
    public void Processor_ExecutesCommandsFromQueue()
    {
        var context = new Dictionary<string, object>();
        new InitProcessorContextCommand(context).Execute();

        var queue = (BlockingCollection<ICommand>)context["queue"];

        var cmd = new Mock<ICommand>();
        queue.Add(cmd.Object);
        // Стоп-команда
        queue.Add(new ActionCommand(() => context["canContinue"] = false));

        var processor = new Processor(new Processable(context));

        // Поток завершился за отведённое время
        Assert.True(processor.Wait(5000));
        // Команда из очереди была выполнена один раз
        cmd.Verify(c => c.Execute(), Times.Once());
    }
}