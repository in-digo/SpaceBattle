namespace SpaceBattle.Lib;

public class MacroCommand : ICommand
{
    private readonly ICommand[] _commands;

    public MacroCommand(params ICommand[] commands)
    {
        _commands = commands;
    }

    public void Execute()
    {
        
    }
}