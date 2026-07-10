using Moq;
using System.Collections.Concurrent;

namespace SpaceBattle.Lib.Tests;

public class InterpretCommandTests : IDisposable
{
    public InterpretCommandTests()
    {
        new InitScopeBasedIoCCommand().Execute();
        IoC.Resolve<ICommand>(
            "Scopes.Current",
            IoC.Resolve<object>("IoC.Scope.Create")
        ).Execute();
    }

    public void Dispose()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Clear").Execute();
    }

    // InterpretCommand находит объект, создаёт команду по operationId и ставит в очередь
    [Fact]
    public void InterpretCommand_Resolves_Object_And_Enqueues_Command()
    {
        // Arrange
        var gameQueue = new BlockingCollection<ICommand>();
        var gameContext = new Dictionary<string, object>
        {
            ["queue"] = gameQueue
        };

        // Мок-объект корабля
        var shipMock = new Mock<IUObject>();
        var ship = shipMock.Object;

        // Регистрируем разрешение игрового объекта по id
        IoC.Resolve<ICommand>(
            "IoC.Register",
            "Game.Objects.Get",
            (Func<object[], object>)(args =>
            {
                var gameCtx = (IDictionary<string, object>)args[0];
                var objectId = (string)args[1];
                return objectId == "548" ? ship : null!;
            })
        ).Execute();

        // Регистрируем регламент: operationId -> имя команды
        IoC.Resolve<ICommand>(
            "IoC.Register",
            "Game.Operation.Resolve",
            (Func<object[], object>)(args =>
            {
                var operationId = (string)args[0];
                return operationId == "move.forward" ? "MoveCommand" : null!;
            })
        ).Execute();

        // Регистрируем создание команды по имени
        IoC.Resolve<ICommand>(
            "IoC.Register",
            "Game.Command.Create",
            (Func<object[], object>)(args =>
            {
                var commandName = (string)args[0];
                var obj = (IUObject)args[1];
                var cmdArgs = (IDictionary<string, object>)args[2];

                if (commandName == "MoveCommand")
                {
                    var velocity = cmdArgs["velocity"];
                    return new ActionCommand(() =>
                    {
                        // Команда движения — для проверки сохраним velocity в объект
                        obj.SetProperty("Velocity", velocity);
                    });
                }

                throw new Exception($"Unknown command: {commandName}");
            })
        ).Execute();

        // Регистрируем постановку команды в очередь игры
        IoC.Resolve<ICommand>(
            "IoC.Register",
            "Queue.Enqueue",
            (Func<object[], object>)(args =>
            {
                var ctx = (IDictionary<string, object>)args[0];
                var cmd = (ICommand)args[1];
                var queue = (BlockingCollection<ICommand>)ctx["queue"];
                return new ActionCommand(() => queue.Add(cmd));
            })
        ).Execute();

        var message = new IncomingMessage
        {
            GameId = "game-123",
            ObjectId = "548",
            OperationId = "move.forward",
            Args = new Dictionary<string, object> { ["velocity"] = 2 }
        };

        var interpretCmd = new InterpretCommand(message, gameContext);

        // Act
        interpretCmd.Execute();

        // Assert — команда движения попала в очередь
        ICommand? enqueuedCmd = null;
        Assert.True(gameQueue.TryTake(out enqueuedCmd, TimeSpan.FromMilliseconds(100)));
        Assert.NotNull(enqueuedCmd);

        // Выполняем команду и проверяем, что velocity установилась
        enqueuedCmd!.Execute();
        shipMock.Verify(s => s.SetProperty("Velocity", 2), Times.Once);
    }
}