using Moq;

namespace SpaceBattle.Lib.Tests;

public class MoveCommandTests
{
    //Для объекта, находящегося в точке (12, 5) и движущегося со скоростью (-7, 3) движение меняет положение объекта на (5, 8)
    [Fact]
    public void Move_ChangesPosition()
    {
        var movable = new Mock<IMovable>();
        movable.SetupGet(m => m.Position).Returns(new Vector(12, 5));
        movable.SetupGet(m => m.Velocity).Returns(new Vector(-7, 3));

        var cmd = new MoveCommand(movable.Object);

        cmd.Execute();

        movable.VerifySet(m => m.Position = new Vector(5, 8), Times.Once);
    }

    //Попытка сдвинуть объект, у которого невозможно прочитать положение в пространстве, приводит к ошибке
    [Fact]
    public void Move_CantGetPosition_Throws()
    {
        var movable = new Mock<IMovable>();
        movable.SetupGet(m => m.Position).Throws<InvalidOperationException>();
        movable.SetupGet(m => m.Velocity).Returns(new Vector(-7, 3));

        var cmd = new MoveCommand(movable.Object);

        Assert.Throws<InvalidOperationException>(() => cmd.Execute());
    }

    //Попытка сдвинуть объект, у которого невозможно прочитать значение мгновенной скорости, приводит к ошибке
    [Fact]
    public void Move_CantGetVelocity_Throws()
    {
        var movable = new Mock<IMovable>();
        movable.SetupGet(m => m.Position).Returns(new Vector(12, 5));
        movable.SetupGet(m => m.Velocity).Throws<InvalidOperationException>();

        var cmd = new MoveCommand(movable.Object);

        Assert.Throws<InvalidOperationException>(() => cmd.Execute());
    }

    //Попытка сдвинуть объект, у которого невозможно изменить положение в пространстве, приводит к ошибке
    [Fact]
    public void Move_CantSetPosition_Throws()
    {
        var movable = new Mock<IMovable>();
        movable.SetupGet(m => m.Position).Returns(new Vector(12, 5));
        movable.SetupGet(m => m.Velocity).Returns(new Vector(-7, 3));
        movable.SetupSet(m => m.Position = It.IsAny<Vector>()).Throws<InvalidOperationException>();

        var cmd = new MoveCommand(movable.Object);

        Assert.Throws<InvalidOperationException>(() => cmd.Execute());
    }
}