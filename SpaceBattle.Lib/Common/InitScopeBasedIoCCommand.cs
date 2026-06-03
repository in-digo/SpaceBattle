using System.Collections.Concurrent;

namespace SpaceBattle.Lib;

public class InitScopeBasedIoCCommand : ICommand
{
    internal static ThreadLocal<object> _currentScope =
        new ThreadLocal<object>(true);

    static ConcurrentDictionary<string, Func<object[], object>> _rootScope =
        new ConcurrentDictionary<string, Func<object[], object>>();

    static ConcurrentDictionary<string, object> _namedScopes =
        new ConcurrentDictionary<string, object>();

    static bool _initialized = false;

    public void Execute()
    {
        if (_initialized) return;

        lock (_rootScope)
        {
            _rootScope.TryAdd(
                "IoC.Scope.Parent",
                args => throw new ArgumentException("Dependency is not found.")
            );
            
            _rootScope.TryAdd(
                "IoC.Scope.Current",
                args => _currentScope.Value ?? _rootScope
            );

            _rootScope.TryAdd(
                "IoC.Scope.Create",
                args =>
                {
                    var parentScope = IoC.Resolve<object>("IoC.Scope.Current");
                    var newScope = new Dictionary<string, Func<object[], object>>
                    {
                        ["IoC.Scope.Parent"] = _ => parentScope
                    };
                    return newScope;
                }
            );

            _rootScope.TryAdd(
                "IoC.Register",
                args => new RegisterDependencyCommand(
                    (string)args[0],
                    (Func<object[], object>)args[1]
                )
            );

            _rootScope.TryAdd(
                "Scopes.Current.Clear",
                args => new ClearCurrentScopeCommand()
            );

            _rootScope.TryAdd(
                "Scopes.New",
                args =>
                {
                    var scopeId = (string)args[0];
                    return new ActionCommand(() =>
                    {
                        var scope = IoC.Resolve<object>("IoC.Scope.Create");
                        _namedScopes[scopeId] = scope;
                    });
                }
            );

            _rootScope.TryAdd(
                "Scopes.Current",
                args => new SetCurrentScopeCommand(args[0])
            );

            IoC.Resolve<ICommand>(
                "Update Ioc Resolve Dependency Strategy",
                (Func<Func<string, object[], object>,
                      Func<string, object[], object>>)(old =>
                    (dependency, args) =>
                    {
                        var scope = _currentScope.Value ?? _rootScope;
                        return new DependencyResolver(scope)
                            .Resolve(dependency, args);
                    }
                )
            ).Execute();

            _initialized = true;
        }
    }
}