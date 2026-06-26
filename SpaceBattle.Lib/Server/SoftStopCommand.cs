using System.Collections.Concurrent;

namespace SpaceBattle.Lib;

public class SoftStopCommand : ICommand
{
    IDictionary<string, object> _context;
    
    public SoftStopCommand(IDictionary<string, object> context)
    {
        _context = context;
    }

    public void Execute()
    {
        var previousProcess = (Action)_context["process"];

        _context["process"] = (Action)(() =>
        {
            previousProcess();

            var queue = (BlockingCollection<ICommand>)_context["queue"];
            if (queue.Count == 0)
            {
                _context["canContinue"] = false;
            }
        });
    }
}