using System.Collections.Concurrent;

namespace SpaceBattle.Lib;

public class NormalState : ICommandProcessingState
{
    private readonly BlockingCollection<ICommand> _queue;

    public NormalState(BlockingCollection<ICommand> queue)
    {
        ArgumentNullException.ThrowIfNull(queue);

        _queue = queue;
    }

    public ICommandProcessingState? Handle()
    {
        var command = _queue.Take();
        command.Execute();

        return this;
    }
}