using Moq;

namespace SpaceBattle.Lib.Tests;

public class PlayerOrderAccessTests : IDisposable
{
    public PlayerOrderAccessTests()
    {
        new InitScopeBasedIoCCommand().Execute();

        IoC.Resolve<ICommand>(
            "Scopes.Current",
            IoC.Resolve<object>("IoC.Scope.Create")
        ).Execute();
    }

    public void Dispose()
    {
        IoC.Resolve<ICommand>(
            "Scopes.Current.Clear"
        ).Execute();
    }

    // Проверяет, что интерпретатор выполняет приказ для собственного объекта игрока,
    // а приказ с идентификатором чужого объекта отклоняется до выполнения действия
    [Fact]
    public void Execute_AllowsOwnObjectAndRejectsAnotherPlayersObject()
    {
        // Arrange
        var playerObject = new Mock<IUObject>().Object;
        var executedObjects = new List<IUObject>();

        IoC.Resolve<ICommand>(
            "IoC.Register",
            "Commands.StartMove",
            (Func<object[], object>)(args =>
            {
                var order = (IUObject)args[0];

                var objectId = (string)order.GetProperty("id");

                var gameObject = IoC.Resolve<IUObject>(
                    "Game.Objects.Get",
                    objectId);

                return new ActionCommand(() => executedObjects.Add(gameObject));
            })
        ).Execute();

        var playerScope = IoC.Resolve<object>("IoC.Scope.Create");

        new ExecuteInScopeCommand(
            playerScope,
            new RegisterPlayerObjectsCommand(
                new Dictionary<string, IUObject>
                {
                    ["ship-1"] = playerObject
                })
        ).Execute();

        var ownObjectOrder = new Mock<IUObject>();
        ownObjectOrder
            .Setup(order => order.GetProperty("action"))
            .Returns("StartMove");
        ownObjectOrder
            .Setup(order => order.GetProperty("id"))
            .Returns("ship-1");

        var anotherPlayersObjectOrder = new Mock<IUObject>();
        anotherPlayersObjectOrder
            .Setup(order => order.GetProperty("action"))
            .Returns("StartMove");
        anotherPlayersObjectOrder
            .Setup(order => order.GetProperty("id"))
            .Returns("ship-2");

        // Act
        new ExecuteInScopeCommand(
            playerScope,
            new InterpretOrderCommand(ownObjectOrder.Object)
        ).Execute();

        var exception = Assert.Throws<UnauthorizedAccessException>(
            () => new ExecuteInScopeCommand(playerScope, new InterpretOrderCommand(anotherPlayersObjectOrder.Object)
            ).Execute());

        // Assert
        Assert.Single(executedObjects);
        Assert.Same(
            playerObject,
            executedObjects[0]);

        Assert.Contains(
            "ship-2",
            exception.Message);
    }
}