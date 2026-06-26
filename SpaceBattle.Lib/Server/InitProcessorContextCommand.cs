using System.Collections.Concurrent;

namespace SpaceBattle.Lib;

public class InitProcessorContextCommand : ICommand
{
    IDictionary<string, object> _context;

    public InitProcessorContextCommand(IDictionary<string, object> context)
    {
        _context = context;
    }

    public void Execute()
    {
        var queue = new BlockingCollection<ICommand>();
        _context["queue"] = queue;
        _context["canContinue"] = true;

        _context["terminate"] = (Action<Exception>)(ex =>
        {
            _context["canContinue"] = false;
            _context["terminateException"] = ex;
        });

        _context["process"] = (Action)(() =>
        {
            var cmd = queue.Take();
            try
            {
                cmd.Execute();
            }
            catch (Exception ex)
            {
                ExceptionHandler.Handle(cmd, ex).Execute();
            }
        });
    }
}