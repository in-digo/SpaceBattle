namespace SpaceBattle.Lib;

public class SetCurrentScopeCommand : ICommand
{
    object _scope;
    public SetCurrentScopeCommand(object scope) => _scope = scope;
    public void Execute() => InitScopeBasedIoCCommand._currentScope.Value = _scope;
}