using Moq;

namespace SpaceBattle.Lib.Tests;

public interface ITestMovable
{
    Vector GetPosition();
    void SetPosition(Vector newValue);
    Vector GetVelocity();
}

public interface ITestFinishable
{
    void Finish();
}

public class AdapterGeneratorTests : IDisposable
{
    const string Iface = "SpaceBattle.Lib.Tests.ITestMovable";

    public AdapterGeneratorTests()
    {
        new InitScopeBasedIoCCommand().Execute();
        IoC.Resolve<ICommand>(
            "Scopes.Current",
            IoC.Resolve<object>("IoC.Scope.Create")
        ).Execute();
        new RegisterAdapterStrategyCommand().Execute();
    }

    public void Dispose()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Clear").Execute();
    }

    // Адаптер, созданный через IoC, реализует запрошенный интерфейс
    [Fact]
    public void Adapter_ImplementsRequestedInterface()
    {
        var obj = new object();
        var adapter = IoC.Resolve<ITestMovable>("Adapter", typeof(ITestMovable), obj);

        Assert.IsAssignableFrom<ITestMovable>(adapter);
    }

    // GetPosition делегирует чтение в IoC по ключу position.get
    [Fact]
    public void GetPosition_ResolvesViaIoC()
    {
        var obj = new object();
        var expected = new Vector(1, 2);

        IoC.Resolve<ICommand>(
            "IoC.Register",
            $"{Iface}:position.get",
            (Func<object[], object>)(args => expected)
        ).Execute();

        var adapter = IoC.Resolve<ITestMovable>("Adapter", typeof(ITestMovable), obj);

        Assert.Equal(expected, adapter.GetPosition());
    }

    // SetPosition резолвит ICommand по ключу position.set и вызывает Execute
    [Fact]
    public void SetPosition_ResolvesCommandAndExecutes()
    {
        var obj = new object();
        var newValue = new Vector(3, 4);
        object[]? capturedArgs = null;
        var command = new Mock<ICommand>();

        IoC.Resolve<ICommand>(
            "IoC.Register",
            $"{Iface}:position.set",
            (Func<object[], object>)(args =>
            {
                capturedArgs = args;
                return command.Object;
            })
        ).Execute();

        var adapter = IoC.Resolve<ITestMovable>("Adapter", typeof(ITestMovable), obj);
        adapter.SetPosition(newValue);

        // Команда была выполнена
        command.Verify(c => c.Execute(), Times.Once());
        // В фабрику пришли целевой объект и новое значение
        Assert.NotNull(capturedArgs);
        Assert.Same(obj, capturedArgs![0]);
        Assert.Equal(newValue, capturedArgs[1]);
    }

    // Finish (void без префикса Get/Set) резолвит ICommand по ключу :finish и выполняет
    [Fact]
    public void Finish_ResolvesCommandAndExecutes()
    {
        const string iface = "SpaceBattle.Lib.Tests.ITestFinishable";
        var obj = new object();
        object[]? capturedArgs = null;
        var command = new Mock<ICommand>();

        IoC.Resolve<ICommand>(
            "IoC.Register",
            $"{iface}:finish",
            (Func<object[], object>)(args =>
            {
                capturedArgs = args;
                return command.Object;
            })
        ).Execute();

        var adapter = IoC.Resolve<ITestFinishable>("Adapter", typeof(ITestFinishable), obj);
        adapter.Finish();

        command.Verify(c => c.Execute(), Times.Once());
        Assert.NotNull(capturedArgs);
        Assert.Same(obj, capturedArgs![0]);
    }
}