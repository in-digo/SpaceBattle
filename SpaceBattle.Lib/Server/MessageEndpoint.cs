using System.Collections.Concurrent;
using System.Text.Json;

namespace SpaceBattle.Lib;

public class MessageEndpoint
{
    private readonly IDictionary<string, IDictionary<string, object>> _games;

    public MessageEndpoint(IDictionary<string, IDictionary<string, object>> games)
    {
        _games = games;
    }

    // Принимает JSON, десериализует, маршрутизирует по gameId, ставит InterpretCommand в очередь игры
    public void Handle(string json)
    {
        // Десериализация через IoC
        var message = IoC.Resolve<IncomingMessage>("Message.Deserialize", json);

        if (message == null || string.IsNullOrEmpty(message.GameId))
            return;

        if (!_games.TryGetValue(message.GameId, out var gameContext))
            return;

        // Создаём InterpretCommand через IoC
        var cmd = IoC.Resolve<ICommand>("Message.Interpret", message, gameContext);

        // Постановка в очередь через IoC
        IoC.Resolve<ICommand>("Queue.Enqueue", gameContext, cmd).Execute();
    }
}