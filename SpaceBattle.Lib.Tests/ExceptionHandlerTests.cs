using Moq;

namespace SpaceBattle.Lib.Tests;

public class ExceptionHandlerTests
{
    //ExceptionHandler возвращает зарегистрированный обработчик для конкретного типа команды и исключения
    [Fact]
    public void Handle_ReturnsRegisteredHandler()
    {
        var cmd = new Mock<ICommand>();
        var ex = new InvalidOperationException();
        var handlerCmd = new Mock<ICommand>();

        ExceptionHandler.Setup();
        ExceptionHandler.Register(
            cmd.Object.GetType(),
            typeof(InvalidOperationException),
            (c, e) => handlerCmd.Object
        );

        var result = ExceptionHandler.Handle(cmd.Object, ex);

        Assert.Equal(handlerCmd.Object, result);
    }
}