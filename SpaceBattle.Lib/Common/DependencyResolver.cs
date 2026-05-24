namespace SpaceBattle.Lib;

public class DependencyResolver
{
    IDictionary<string, Func<object[], object>> _dependencies;

    public DependencyResolver(object scope)
    {
        _dependencies = (IDictionary<string, Func<object[], object>>)scope;
    }

    public object Resolve(string dependency, object[] args)
    {
        var current = _dependencies;
        while (true)
        {
            if (current.TryGetValue(dependency, out var factory))
                return factory(args);
            current = (IDictionary<string, Func<object[], object>>)
                current["IoC.Scope.Parent"](args);
        }
    }
}