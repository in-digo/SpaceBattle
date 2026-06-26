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
        // Получаем игровой объект по objectId
        var obj = IoC.Resolve<IUObject>("Game.Objects.Get", _gameContext, _message.ObjectId);

        // По operationId определяем имя команды через регламент
        var commandName = IoC.Resolve<string>("Game.Operation.Resolve", _message.OperationId);

        // Создаём команду с параметрами из args
        var cmd = IoC.Resolve<ICommand>("Game.Command.Create", commandName, obj, _message.Args);

        // Ставим команду в очередь игры
        IoC.Resolve<ICommand>("Queue.Enqueue", _gameContext, cmd).Execute();
    }
}