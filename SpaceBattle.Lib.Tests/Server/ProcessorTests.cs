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

    // Исключение из команды не прерывает поток
    [Fact]
    public void Processor_ContinuesAfterCommandThrows()
    {
        ExceptionHandler.Setup();

        var context = new Dictionary<string, object>();
        new InitProcessorContextCommand(context).Execute();

        var queue = (BlockingCollection<ICommand>)context["queue"];

        var throwing = new Mock<ICommand>();
        throwing.Setup(c => c.Execute()).Throws(new Exception());

        // Cтратегия гасит исключение, цикл продолжает работу
        ExceptionHandler.Register(
            throwing.Object.GetType(),
            typeof(Exception),
            (cmd, ex) => new ActionCommand(() => { })
        );

        var after = new Mock<ICommand>();

        queue.Add(throwing.Object);
        queue.Add(after.Object);
        queue.Add(new ActionCommand(() => context["canContinue"] = false));

        var processor = new Processor(new Processable(context));

        Assert.True(processor.Wait(5000));
        // Команда, выбросившая исключение, была вызвана
        throwing.Verify(c => c.Execute(), Times.Once());
        // Команда после неё всё равно выполнилась
        after.Verify(c => c.Execute(), Times.Once());
    }

    // После команды Старт поток запущен
    [Fact]
    public void StartThreadCommand_StartsThread()
    {
        var context = new Dictionary<string, object>();
        new InitProcessorContextCommand(context).Execute();

        var queue = (BlockingCollection<ICommand>)context["queue"];

        var started = new ManualResetEventSlim(false);
        // Команда, по которой поток сигналит, что он стартовал
        queue.Add(new ActionCommand(() => started.Set()));

        new StartThreadCommand(context).Execute();

        // Ожидание сигнала
        Assert.True(started.Wait(5000));

        // Завершаем поток
        queue.Add(new ActionCommand(() => context["canContinue"] = false));
        Assert.True(((Processor)context["processor"]).Wait(5000));
    }
}