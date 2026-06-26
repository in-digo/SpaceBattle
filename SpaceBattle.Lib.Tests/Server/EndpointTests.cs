using System.Collections.Concurrent;
using System.Text.Json;

namespace SpaceBattle.Lib.Tests;

public class EndpointTests : IDisposable
{
    public EndpointTests()
    {
        new InitScopeBasedIoCCommand().Execute();
        IoC.Resolve<ICommand>(
            "Scopes.Current",
            IoC.Resolve<object>("IoC.Scope.Create")
        ).Execute();

        // Регистрируем десериализацию сообщения
        IoC.Resolve<ICommand>(
            "IoC.Register",
            "Message.Deserialize",
            (Func<object[], object>)(args =>
            {
                var json = (string)args[0];
                return JsonSerializer.Deserialize<IncomingMessage>(json)!;
            })
        ).Execute();

        // Регистрируем получение очереди из контекста игры
        IoC.Resolve<ICommand>(
            "IoC.Register",
            "Game.Queue",
            (Func<object[], object>)(args =>
            {
                var ctx = (IDictionary<string, object>)args[0];
                return ctx["queue"];
            })
        ).Execute();

        // Регистрируем создание InterpretCommand
        IoC.Resolve<ICommand>(
            "IoC.Register",
            "Message.Interpret",
            (Func<object[], object>)(args =>
            {
                var msg = (IncomingMessage)args[0];
                var ctx = (IDictionary<string, object>)args[1];
                return new InterpretCommand(msg, ctx);
            })
        ).Execute();

        // Регистрируем постановку в очередь
        IoC.Resolve<ICommand>(
            "IoC.Register",
            "Queue.Enqueue",
            (Func<object[], object>)(args =>
            {
                // args[0] — gameContext, args[1] — команда
                var ctx = (IDictionary<string, object>)args[0];
                var cmd = (ICommand)args[1];
                var queue = (BlockingCollection<ICommand>)ctx["queue"];
                return new ActionCommand(() => queue.Add(cmd));
            })
        ).Execute();
    }

    public void Dispose()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Clear").Execute();
    }

    // Endpoint десериализует сообщение, находит игру и ставит InterpretCommand в очередь игры
    [Fact]
    public void Endpoint_Receives_Message_And_Enqueues_InterpretCommand()
    {
        // Arrange
        var json = @"{
            ""gameId"": ""game-123"",
            ""objectId"": ""548"",
            ""operationId"": ""move.forward"",
            ""args"": { ""velocity"": 2 }
        }";

        var gameContext = new Dictionary<string, object>();
        var queue = new BlockingCollection<ICommand>();
        gameContext["queue"] = queue;

        var games = new Dictionary<string, IDictionary<string, object>>
        {
            ["game-123"] = gameContext
        };

        var endpoint = new MessageEndpoint(games);

        // Act
        endpoint.Handle(json);

        // Assert - команда попала в очередь игры
        ICommand? enqueuedCmd = null;
        Assert.True(queue.TryTake(out enqueuedCmd, TimeSpan.FromMilliseconds(100)));
        Assert.NotNull(enqueuedCmd);
        Assert.IsType<InterpretCommand>(enqueuedCmd);
    }
}