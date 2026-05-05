using Moq;

namespace SpaceBattle.Lib.Tests;

public class RetryTwiceThenLogTests
{
    //Повторить два раза, потом записать в лог
    [Fact]
    public void FirstTime_Retry_SecondTime_Retry_ThirdTime_Log()
    {
        var queue = new Queue<ICommand>();
        var log = new Mock<ILog>();
        var cmd = new Mock<ICommand>();
        cmd.Setup(c => c.Execute()).Throws<InvalidOperationException>();

        ExceptionHandler.Setup();

        //Исключение от оригинальной команды - ретрай
        ExceptionHandler.Register(
            cmd.Object.GetType(),
            typeof(InvalidOperationException),
            (c, e) => new RetryCommand(c)
        );

        //Исключение от RetryCommand - второй ретрай
        ExceptionHandler.Register(
            typeof(RetryCommand),
            typeof(InvalidOperationException),
            (c, e) => new RetryTwoCommand(c)
        );

        //Исключение от RetryTwoCommand - запись в лог
        ExceptionHandler.Register(
            typeof(RetryTwoCommand),
            typeof(InvalidOperationException),
            (c, e) => new LogCommand(e, log.Object)
        );

        //Исключение при вызове оригинальной команды
        try 
        { 
            cmd.Object.Execute(); 
        }
        catch (Exception ex)
        {
            queue.Enqueue(ExceptionHandler.Handle(cmd.Object, ex));
        }

        //Исключение при вызове ретрая
        var retryCmd = queue.Dequeue();
        try 
        { 
            retryCmd.Execute(); 
        }
        catch (Exception ex)
        {
            queue.Enqueue(ExceptionHandler.Handle(retryCmd, ex));
        }

        ///Исключение при вызове второго ретрая
        var retryTwoCmd = queue.Dequeue();
        try 
        { 
            retryTwoCmd.Execute(); 
        }
        catch (Exception ex)
        {
            queue.Enqueue(ExceptionHandler.Handle(retryTwoCmd, ex));
        }

        //Запись в лог
        var logCmd = queue.Dequeue();
        logCmd.Execute();

        log.Verify(l => l.Write(It.IsAny<Exception>()), Times.Once);
        Assert.Empty(queue);
    }
}