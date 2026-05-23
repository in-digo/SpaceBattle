namespace SpaceBattle.Lib;

public class IoC
{
    internal static Func<string, object[], object> _strategy = (dependency, args) =>
    {
        if (dependency == "Update Ioc Resolve Dependency Strategy")
        {
            return new UpdateIocResolveDependencyStrategyCommand(
                (Func<Func<string, object[], object>,
                        Func<string, object[], object>>)args[0]
            );
        }

        throw new ArgumentException($"Dependency '{dependency}' is not found.");        
    };

    public static T Resolve<T>(string dependency, params object[] args)
    {
        return (T)_strategy(dependency, args);
    }
}