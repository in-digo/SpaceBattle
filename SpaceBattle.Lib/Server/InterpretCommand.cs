namespace SpaceBattle.Lib;

public class InterpretCommand : ICommand
{
    private readonly IncomingMessage _message;
    private readonly IDictionary<string, object> _gameContext;

    public InterpretCommand(IncomingMessage message, IDictionary<string, object> gameContext)
    {
        _message = message;
        _gameContext = gameContext;
    }

    public void Execute()
    {
        // Пока ничего не делает
    }
}