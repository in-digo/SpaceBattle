namespace SpaceBattle.Lib;

public class StartThreadCommand : ICommand
{
    IDictionary<string, object> _context;

    public StartThreadCommand(IDictionary<string, object> context)
    {
        _context = context;
    }

    public void Execute()
    {
        _context["processor"] = new Processor(new Processable(_context));
    }
}