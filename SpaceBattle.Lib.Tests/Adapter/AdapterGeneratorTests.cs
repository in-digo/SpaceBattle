namespace SpaceBattle.Lib.Tests;

public interface ITestMovable
{
    Vector GetPosition();
    void SetPosition(Vector newValue);
    Vector GetVelocity();
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
}