using Moq;

namespace SpaceBattle.Lib.Tests;

public class RotateWithChangeVelocityTests
{
    //Объект вращается и движется - направление и скорость меняются
    [Fact]
    public void RotatingMovingObject_ChangesDirectionAndVelocity()
    {
        var rotatable = new Mock<IRotatable>();
        rotatable.SetupProperty(r => r.Direction, 0);
        rotatable.SetupGet(r => r.AngularVelocity).Returns(1);
        rotatable.SetupGet(r => r.DirectionsCount).Returns(4);

        var movable = new Mock<IMovable>();
        movable.SetupProperty(m => m.Velocity, new Vector(10, 0));

        var cmd = new MacroCommand(
            new RotateCommand(rotatable.Object),
            new ChangeVelocityCommand(movable.Object, rotatable.Object)
        );

        cmd.Execute();

        Assert.Equal(1, rotatable.Object.Direction);
        Assert.Equal(new Vector(0, 10), movable.Object.Velocity);
    }

    //Объект только вращается (не движется) - только RotateCommand
    [Fact]
    public void RotatingOnlyObject_ChangesDirectionOnly()
    {
        var rotatable = new Mock<IRotatable>();
        rotatable.SetupProperty(r => r.Direction, 3);
        rotatable.SetupGet(r => r.AngularVelocity).Returns(2);
        rotatable.SetupGet(r => r.DirectionsCount).Returns(8);

        var cmd = new MacroCommand(
            new RotateCommand(rotatable.Object)
        );

        cmd.Execute();

        Assert.Equal(5, rotatable.Object.Direction);
    }
}