namespace SpaceBattle.Lib;

public class RegisterDependencyCommand : ICommand
{
    string _dependency;
    Func<object[], object> _factory;

    public RegisterDependencyCommand(string dependency, Func<object[], object> factory)
    {
        _dependency = dependency;
        _factory = factory;
    }

    public void Execute()
    {
        var scope = IoC.Resolve<IDictionary<string, Func<object[], object>>>("IoC.Scope.Current");
        scope[_dependency] = _factory;
    }
}