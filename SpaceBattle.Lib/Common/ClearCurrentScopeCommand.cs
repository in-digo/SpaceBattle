namespace SpaceBattle.Lib;

public class ClearCurrentScopeCommand : ICommand
{
    public void Execute()
    {
        InitScopeBasedIoCCommand._currentScope.Value = null!;
    }
}