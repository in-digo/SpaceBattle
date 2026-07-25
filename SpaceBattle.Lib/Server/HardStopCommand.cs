namespace SpaceBattle.Lib;

public class HardStopCommand  : IStateTransitionCommand
{
    private readonly IDictionary<string, object> _context;

    public HardStopCommand(IDictionary<string, object> context)
    {
        _context = context;
    }

    public ICommandProcessingState? NextState => null;

    public void Execute()
    {
        _context["canContinue"] = false;
    }
}