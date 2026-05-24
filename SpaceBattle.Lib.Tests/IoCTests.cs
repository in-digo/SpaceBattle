namespace SpaceBattle.Lib.Tests;

public class IoCTests : IDisposable
{
    public IoCTests()
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

    //Зависимость регистрируется в скоупе и разрешается
    [Fact]
    public void IoC_Register_MakesDependencyResolvable()
    {
        var expected = new object();
        IoC.Resolve<ICommand>(
            "IoC.Register",
            "test.obj",
            (Func<object[], object>)(args => expected)
        ).Execute();

        var actual = IoC.Resolve<object>("test.obj");
        Assert.Same(expected, actual);
    }

    //Попытка разрешить несуществующую зависимость - выбрасывается исключение
    [Fact]
    public void Resolve_UnregisteredDependency_ThrowsException()
    {
        Assert.ThrowsAny<Exception>(
            () => IoC.Resolve<object>("nonexistent.dependency")
        );
    }

    //Зависимость из родительского скоупа доступна в дочернем
    [Fact]
    public void Resolve_ParentScopeDependency_IsAccessibleInChildScope()
    {
        IoC.Resolve<ICommand>(
            "IoC.Register",
            "parent.val",
            (Func<object[], object>)(args => (object)42)
        ).Execute();

        var childScope = IoC.Resolve<object>("IoC.Scope.Create");
        IoC.Resolve<ICommand>("Scopes.Current", childScope).Execute();

        Assert.Equal(42, IoC.Resolve<int>("parent.val"));
    }

    //Разные потоки работают в разных скоупах - каждый видит своё значение
    [Fact]
    public void DifferentThreads_HaveIndependentScopes()
    {
        int? resultThread1 = null;
        int? resultThread2 = null;
        var barrier = new Barrier(2);

        var t1 = new Thread(() =>
        {
            new InitScopeBasedIoCCommand().Execute();
            var scope = IoC.Resolve<object>("IoC.Scope.Create");
            IoC.Resolve<ICommand>("Scopes.Current", scope).Execute();
            IoC.Resolve<ICommand>("IoC.Register", "val", (Func<object[], object>)(_ => 1)).Execute();
            barrier.SignalAndWait();
            resultThread1 = IoC.Resolve<int>("val");
            IoC.Resolve<ICommand>("Scopes.Current.Clear").Execute();
        });

        var t2 = new Thread(() =>
        {
            new InitScopeBasedIoCCommand().Execute();
            var scope = IoC.Resolve<object>("IoC.Scope.Create");
            IoC.Resolve<ICommand>("Scopes.Current", scope).Execute();
            IoC.Resolve<ICommand>("IoC.Register", "val", (Func<object[], object>)(_ => 2)).Execute();
            barrier.SignalAndWait();
            resultThread2 = IoC.Resolve<int>("val");
            IoC.Resolve<ICommand>("Scopes.Current.Clear").Execute();
        });

        t1.Start();
        t2.Start();
        t1.Join();
        t2.Join();

        Assert.Equal(1, resultThread1);
        Assert.Equal(2, resultThread2);
    }
}