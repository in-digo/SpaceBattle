namespace SpaceBattle.Lib.Tests;

public class IoCTests
{
    //Подмена стратегии через Update Ioc Resolve Dependency Strategy - новая зависимость становится доступной
    [Fact]
    public void UpdateStrategy_NewDependencyBecomesResolvable()
    {
        var expected = new object();

        IoC.Resolve<ICommand>(
            "Update Ioc Resolve Dependency Strategy",
            (Func<Func<string, object[], object>, Func<string, object[], object>>)(old =>
                (key, args) =>
                {
                    if (key == "test.dependency")
                        return expected;
                    return old(key, args);
                }
            )
        ).Execute();

        var actual = IoC.Resolve<object>("test.dependency");

        Assert.Same(expected, actual);
    }
}