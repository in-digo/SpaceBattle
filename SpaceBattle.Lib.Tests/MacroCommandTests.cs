using Moq;

namespace SpaceBattle.Lib.Tests;

public class MacroCommandTests
{
    //Успешное выполнение всех команд
    [Fact]
    public void MacroCommand_Success()
    {
        var cmd1 = new Mock<ICommand>();
        var cmd2 = new Mock<ICommand>();
        var cmd3 = new Mock<ICommand>();

        var macroCmd = new MacroCommand(cmd1.Object, cmd2.Object, cmd3.Object);

        macroCmd.Execute();

        cmd1.Verify(c => c.Execute(), Times.Once);
        cmd2.Verify(c => c.Execute(), Times.Once);
        cmd3.Verify(c => c.Execute(), Times.Once);
    }

    //Пустой список команд - без исключения
    [Fact]
    public void MacroCommand_EmptyList_NoException()
    {
        var macroCmd = new MacroCommand();

        macroCmd.Execute();
    }

    //Вторая команда бросает исключение, третья не выполняется, MacroCommand бросает CommandException
    [Fact]
    public void SecondCommandFails_ThirdNotExecuted_ThrowsCommandException()
    {
        var cmd1 = new Mock<ICommand>();
        var cmd2 = new Mock<ICommand>();
        var cmd3 = new Mock<ICommand>();
        cmd2.Setup(c => c.Execute()).Throws<InvalidOperationException>();

        var macroCmd = new MacroCommand(cmd1.Object, cmd2.Object, cmd3.Object);

        var ex = Assert.Throws<CommandException>(() => macroCmd.Execute());
        Assert.IsType<InvalidOperationException>(ex.InnerException);
        cmd3.Verify(c => c.Execute(), Times.Never);
    }
}