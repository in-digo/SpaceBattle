using Moq;

namespace SpaceBattle.Lib.Tests;

public class InterpretOrderCommandTests : IDisposable
{
    public InterpretOrderCommandTests()
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

    // Проверяет, что интерпретатор по действию из приказа получает команду через IoC, передаёт ей исходный приказ и выполняет её
    [Theory]
    [InlineData("StartMove")]
    [InlineData("StopMove")]
    [InlineData("Shoot")]
    [InlineData("PublishStatistics")]
    public void Execute_ResolvesAndExecutesCommandRegisteredForAction(string action)
    {
        // Arrange
        var orderMock = new Mock<IUObject>();
        orderMock
            .Setup(order => order.GetProperty("action"))
            .Returns(action);

        var expectedCommandMock = new Mock<ICommand>();
        IUObject? receivedOrder = null;

        IoC.Resolve<ICommand>(
            "IoC.Register",
            $"Commands.{action}",
            (Func<object[], object>)(args =>
            {
                receivedOrder = (IUObject)args[0];
                return expectedCommandMock.Object;
            })
        ).Execute();

        var command = new InterpretOrderCommand(orderMock.Object);

        // Act
        command.Execute();

        // Assert
        Assert.Same(orderMock.Object, receivedOrder);
        expectedCommandMock.Verify(
            expectedCommand => expectedCommand.Execute(),
            Times.Once);
    }
}