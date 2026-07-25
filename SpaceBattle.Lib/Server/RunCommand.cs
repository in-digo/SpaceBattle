namespace SpaceBattle.Lib;

public class RunCommand : IStateTransitionCommand
{
    private readonly ICommandProcessingState _nextState;

    public RunCommand(ICommandProcessingState nextState)
    {
        ArgumentNullException.ThrowIfNull(nextState);

        _nextState = nextState;
    }

    public ICommandProcessingState? NextState => _nextState;

    public void Execute()
    {
    }
}