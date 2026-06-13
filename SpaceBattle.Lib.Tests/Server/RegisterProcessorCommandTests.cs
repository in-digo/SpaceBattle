using System.Collections.Concurrent;

namespace SpaceBattle.Lib.Tests;

public class RegisterProcessorCommandTests : IDisposable
{
    public RegisterProcessorCommandTests()
    {
        new InitScopeBasedIoCCommand().Execute();
        IoC.Resolve<ICommand>(
            "Scopes.Current",
            IoC.Resolve<object>("IoC.Scope.Create")
        ).Execute();
    }

    public void Dispose()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Clear").Execute();
    }

    // После регистрации процессор-контекст доступен по ключу "Server.CommandProcessor" через IoC,
    // запущенный поток гасится hard stop'ом
    [Fact]
    public void Processor_IsResolvableViaIoC()
    {
        new RegisterProcessorCommand().Execute();

        var context = IoC.Resolve<IDictionary<string, object>>("Server.CommandProcessor");
        var processor = (Processor)context["processor"];
        var queue = (BlockingCollection<ICommand>)context["queue"];

        // hard stop, чтобы поток завершился
        queue.Add(new HardStopCommand(context));

        Assert.True(processor.Wait(5000));
    }
}