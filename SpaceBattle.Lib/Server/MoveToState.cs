using System.Collections.Concurrent;

namespace SpaceBattle.Lib;

public class MoveToState : ICommandProcessingState
{
    private readonly BlockingCollection<ICommand> _sourceQueue;
    private readonly BlockingCollection<ICommand> _targetQueue;

    public MoveToState(
        BlockingCollection<ICommand> sourceQueue,
        BlockingCollection<ICommand> targetQueue)
    {
        ArgumentNullException.ThrowIfNull(sourceQueue);
        ArgumentNullException.ThrowIfNull(targetQueue);

        _sourceQueue = sourceQueue;
        _targetQueue = targetQueue;
    }

    public ICommandProcessingState? Handle()
    {
        var command = _sourceQueue.Take();
        _targetQueue.Add(command);

        return this;
    }
}