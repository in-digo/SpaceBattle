namespace SpaceBattle.Lib;

public class UpdateIocResolveDependencyStrategyCommand : ICommand
{
    private readonly Func<Func<string, object[], object>, Func<string, object[], object>> _updater;

    public UpdateIocResolveDependencyStrategyCommand(
        Func<Func<string, object[], object>, 
        Func<string, object[], object>> updater)
    {
        _updater = updater;
    }

    public void Execute()
    {
        IoC._strategy = _updater(IoC._strategy);
    }
}