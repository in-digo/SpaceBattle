using Moq;

namespace SpaceBattle.Lib.Tests;

public class LogAndRetryTests
{
    //LogCommand записывает информацию об исключении в лог
    [Fact]
    public void LogCommand_WritesToLog()
    {
        var ex = new InvalidOperationException("test error");
        var log = new Mock<ILog>();

        var cmd = new LogCommand(ex, log.Object);

        cmd.Execute();

        log.Verify(l => l.Write(ex), Times.Once);
    }

    //Обработчик исключения ставит LogCommand в очередь команд
    [Fact]
    public void Handler_PutsLogCommandInQueue()
    {
        var queue = new Queue<ICommand>();
        var cmd = new Mock<ICommand>();
        var ex = new InvalidOperationException();

        ExceptionHandler.Setup();
        ExceptionHandler.Register(
            cmd.Object.GetType(),
            typeof(InvalidOperationException),
            (c, e) => new LogCommand(e, new Mock<ILog>().Object)
        );

        var result = ExceptionHandler.Handle(cmd.Object, ex);
        queue.Enqueue(result);

        Assert.Single(queue);
        Assert.IsType<LogCommand>(queue.Peek());
    }

    //RetryCommand повторяет команду, выбросившую исключение
    [Fact]
    public void RetryCommand_ExecutesOriginalCommand()
    {
        var cmd = new Mock<ICommand>();

        var retry = new RetryCommand(cmd.Object);

        retry.Execute();

        cmd.Verify(c => c.Execute(), Times.Once);
    }

    //Обработчик исключения ставит RetryCommand в очередь команд
    [Fact]
    public void Handler_PutsRetryCommandInQueue()
    {
        var queue = new Queue<ICommand>();
        var cmd = new Mock<ICommand>();
        var ex = new InvalidOperationException();

        ExceptionHandler.Setup();
        ExceptionHandler.Register(
            cmd.Object.GetType(),
            typeof(InvalidOperationException),
            (c, e) => new RetryCommand(c)
        );

        var result = ExceptionHandler.Handle(cmd.Object, ex);
        queue.Enqueue(result);

        Assert.Single(queue);
        Assert.IsType<RetryCommand>(queue.Peek());
    }
}