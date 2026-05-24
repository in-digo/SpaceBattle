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
}