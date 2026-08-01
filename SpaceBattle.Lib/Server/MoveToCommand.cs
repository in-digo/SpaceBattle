namespace SpaceBattle.Lib;

public class MoveToCommand : IStateTransitionCommand
{
    private readonly ICommandProcessingState _nextState;

    public MoveToCommand(ICommandProcessingState nextState)
    {
        ArgumentNullException.ThrowIfNull(nextState);

        _nextState = nextState;
    }

    public ICommandProcessingState? NextState => _nextState;

    public void Execute()
    {
    }
}