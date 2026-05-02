using Moq;

namespace SpaceBattle.Lib.Tests;

public class RotateCommandTests
{
    //Для объекта с направлением 6, угл. скоростью 3, 8 возможными направлениями поворот меняет направление объекта на 1
    [Fact]
    public void Rotate_ChangesDirection()
    {
        var rotatable = new Mock<IRotatable>();
        rotatable.SetupGet(r => r.Direction).Returns(6);
        rotatable.SetupGet(r => r.AngularVelocity).Returns(3);
        rotatable.SetupGet(r => r.DirectionsCount).Returns(8);

        var cmd = new RotateCommand(rotatable.Object);

        cmd.Execute();

        rotatable.VerifySet(r => r.Direction = 1, Times.Once);
    }

    //Попытка повернуть объект, у которого невозможно прочитать направление, приводит к ошибке
    [Fact]
    public void Rotate_CantGetDirection_Throws()
    {
        var rotatable = new Mock<IRotatable>();
        rotatable.SetupGet(r => r.Direction).Throws<InvalidOperationException>();
        rotatable.SetupGet(r => r.AngularVelocity).Returns(3);
        rotatable.SetupGet(r => r.DirectionsCount).Returns(8);

        var cmd = new RotateCommand(rotatable.Object);

        Assert.Throws<InvalidOperationException>(() => cmd.Execute());
    }

    //Попытка повернуть объект, у которого невозможно прочитать значение угл. скорости, приводит к ошибке
    [Fact]
    public void Rotate_CantGetAngularVelocity_Throws()
    {
        var rotatable = new Mock<IRotatable>();
        rotatable.SetupGet(r => r.Direction).Returns(6);
        rotatable.SetupGet(r => r.AngularVelocity).Throws<InvalidOperationException>();
        rotatable.SetupGet(r => r.DirectionsCount).Returns(8);

        var cmd = new RotateCommand(rotatable.Object);

        Assert.Throws<InvalidOperationException>(() => cmd.Execute());
    }

    //Попытка повернуть объект, у которого невозможно изменить направление, приводит к ошибке
    [Fact]
    public void Rotate_CantSetDirection_Throws()
    {
        var rotatable = new Mock<IRotatable>();
        rotatable.SetupGet(r => r.Direction).Returns(6);
        rotatable.SetupGet(r => r.AngularVelocity).Returns(3);
        rotatable.SetupGet(r => r.DirectionsCount).Returns(8);
        rotatable.SetupSet(r => r.Direction = It.IsAny<int>()).Throws<InvalidOperationException>();

        var cmd = new RotateCommand(rotatable.Object);

        Assert.Throws<InvalidOperationException>(() => cmd.Execute());
    }
}