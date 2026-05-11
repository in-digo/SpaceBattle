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
        try
        {
            foreach (var cmd in _commands)
                cmd.Execute();
        }
        catch (Exception ex)
        {
            throw new CommandException("Ошибка MacroCommand", ex);
        }
    }
}