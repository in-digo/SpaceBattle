namespace SpaceBattle.Lib;

public class ExecuteInScopeCommand : ICommand
{
    private readonly object _scope;
    private readonly ICommand _command;

    public ExecuteInScopeCommand(object scope, ICommand command)
    {
        _scope = scope;
        _command = command;
    }

    public void Execute()
    {
        var previousScope = IoC.Resolve<object>("IoC.Scope.Current");

        IoC.Resolve<ICommand>(
            "Scopes.Current",
            _scope
        ).Execute();

        try
        {
            _command.Execute();
        }
        finally
        {
            IoC.Resolve<ICommand>(
                "Scopes.Current",
                previousScope
            ).Execute();
        }
    }
}