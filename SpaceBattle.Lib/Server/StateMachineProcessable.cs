namespace SpaceBattle.Lib;

public class StateMachineProcessable : IProcessable
{
    private ICommandProcessingState? _currentState;
    private readonly Action<Exception> _terminate;

    public StateMachineProcessable(
        ICommandProcessingState initialState,
        Action<Exception> terminate)
    {
        ArgumentNullException.ThrowIfNull(initialState);
        ArgumentNullException.ThrowIfNull(terminate);

        _currentState = initialState;
        _terminate = terminate;
    }

    public bool CanContinue => _currentState != null;

    public void Process()
    {
        _currentState = _currentState?.Handle();
    }

    public void Terminate(Exception ex)
    {
        _currentState = null;
        _terminate(ex);
    }
}