using Moq;

namespace SpaceBattle.Lib.Tests;

public class RegisterPlayerObjectsCommandTests : IDisposable
{
    public RegisterPlayerObjectsCommandTests()
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

    // Проверяет, что каждому игроку в его IoC-скоупе доступны только его игровые объекты,
    // а попытка получить объект другого игрока завершается отказом в доступе
    [Fact]
    public void Execute_IsolatesObjectsOfDifferentPlayers()
    {
        // Arrange
        var firstPlayerObject = new Mock<IUObject>().Object;
        var secondPlayerObject = new Mock<IUObject>().Object;

        var firstPlayerScope = IoC.Resolve<object>("IoC.Scope.Create");
        var secondPlayerScope = IoC.Resolve<object>("IoC.Scope.Create");

        var firstPlayerObjects =
            new Dictionary<string, IUObject>
            {
                ["ship-1"] = firstPlayerObject
            };

        var secondPlayerObjects =
            new Dictionary<string, IUObject>
            {
                ["ship-2"] = secondPlayerObject
            };

        // Act
        new ExecuteInScopeCommand(
            firstPlayerScope,
            new RegisterPlayerObjectsCommand(firstPlayerObjects)
        ).Execute();

        new ExecuteInScopeCommand(
            secondPlayerScope,
            new RegisterPlayerObjectsCommand(secondPlayerObjects)
        ).Execute();

        // Assert
        new ExecuteInScopeCommand(
            firstPlayerScope,
            new ActionCommand(() =>
            {
                Assert.Same(
                    firstPlayerObject,
                    IoC.Resolve<IUObject>(
                        "Game.Objects.Get",
                        "ship-1"));

                Assert.Throws<UnauthorizedAccessException>(
                    () => IoC.Resolve<IUObject>(
                        "Game.Objects.Get",
                        "ship-2"));
            })
        ).Execute();

        new ExecuteInScopeCommand(
            secondPlayerScope,
            new ActionCommand(() =>
            {
                Assert.Same(
                    secondPlayerObject,
                    IoC.Resolve<IUObject>(
                        "Game.Objects.Get",
                        "ship-2"));

                Assert.Throws<UnauthorizedAccessException>(
                    () => IoC.Resolve<IUObject>(
                        "Game.Objects.Get",
                        "ship-1"));
            })
        ).Execute();
    }
}