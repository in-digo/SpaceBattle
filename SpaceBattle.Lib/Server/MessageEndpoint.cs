namespace SpaceBattle.Lib;

public class MessageEndpoint : IMessageEndpoint
{
    // Принимает JSON, десериализует, маршрутизирует по gameId, ставит InterpretCommand в очередь игры
    public void Handle(string json)
    {
        var message = IoC.Resolve<IncomingMessage>("Message.Deserialize", json);

        if (message == null || string.IsNullOrEmpty(message.GameId))
            return;

        var games = IoC.Resolve<IDictionary<string, IDictionary<string, object>>>("Games.Storage");

        if (!games.TryGetValue(message.GameId, out var gameContext))
            return;

        var cmd = IoC.Resolve<ICommand>("Message.Interpret", message, gameContext);

        IoC.Resolve<ICommand>("Queue.Enqueue", gameContext, cmd).Execute();
    }
}