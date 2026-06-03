namespace SpaceBattle.Lib;

public class ActionCommand : ICommand
{
    Action _action;
    public ActionCommand(Action action) => _action = action;
    public void Execute() => _action();
}