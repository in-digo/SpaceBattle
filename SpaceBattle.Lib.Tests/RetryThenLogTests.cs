using Moq;

namespace SpaceBattle.Lib.Tests;

public class RetryThenLogTests
{
    //При первом выбросе исключения команда повторяется, при повторном выбросе исключения информация записывается в лог
    [Fact]
    public void FirstTime_Retry_SecondTime_Log()
    {
        var queue = new Queue<ICommand>();
        var log = new Mock<ILog>();
        var cmd = new Mock<ICommand>();
        cmd.Setup(c => c.Execute()).Throws<InvalidOperationException>();

        ExceptionHandler.Setup();

        //Исключение от оригинальной команды - повторить
        ExceptionHandler.Register(
            cmd.Object.GetType(),
            typeof(InvalidOperationException),
            (c, e) => new RetryCommand(c)
        );

        //Исключение от RetryCommand - алогировать
        ExceptionHandler.Register(
            typeof(RetryCommand),
            typeof(InvalidOperationException),
            (c, e) => new LogCommand(e, log.Object)
        );

        //Исключение при вызове команды
        try
        {
            cmd.Object.Execute();
        }
        catch (Exception ex)
        {
            queue.Enqueue(ExceptionHandler.Handle(cmd.Object, ex));
        }

        //Retry
        var retryCmd = queue.Dequeue();
        try
        {
            retryCmd.Execute();
        }
        catch (Exception ex)
        {
            queue.Enqueue(ExceptionHandler.Handle(retryCmd, ex));
        }

        //Запись в лог
        var logCmd = queue.Dequeue();
        logCmd.Execute();

        log.Verify(l => l.Write(It.IsAny<Exception>()), Times.Once);
        Assert.Empty(queue);
    }
}