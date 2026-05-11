using Moq;

namespace SpaceBattle.Lib.Tests;

public class ChangeVelocityCommandTests
{
    //Поворот на 90 градусов - вектор (10, 0) -> (0, 10)
    [Fact]
    public void Rotate90Degrees_VelocityRotated()
    {
        var movable = new Mock<IMovable>();
        movable.SetupProperty(m => m.Velocity, new Vector(10, 0));

        var rotatable = new Mock<IRotatable>();
        rotatable.SetupGet(r => r.AngularVelocity).Returns(1);
        rotatable.SetupGet(r => r.DirectionsCount).Returns(4);

        var cmd = new ChangeVelocityCommand(movable.Object, rotatable.Object);

        cmd.Execute();

        Assert.Equal(new Vector(0, 10), movable.Object.Velocity);
    }

    //Нулевой поворот - вектор не меняется
    [Fact]
    public void ZeroAngularVelocity_VelocityUnchanged()
    {
        var movable = new Mock<IMovable>();
        movable.SetupProperty(m => m.Velocity, new Vector(5, -3));

        var rotatable = new Mock<IRotatable>();
        rotatable.SetupGet(r => r.AngularVelocity).Returns(0);
        rotatable.SetupGet(r => r.DirectionsCount).Returns(4);
 
        var cmd = new ChangeVelocityCommand(movable.Object, rotatable.Object);

        cmd.Execute();

        Assert.Equal(new Vector(5, -3), movable.Object.Velocity);
    }

    //Полный оборот (360 градусов) - вектор не меняется
    [Fact]
    public void FullRotation_VelocityUnchanged()
    {
        var movable = new Mock<IMovable>();
        movable.SetupProperty(m => m.Velocity, new Vector(7, 2));

        var rotatable = new Mock<IRotatable>();
        rotatable.SetupGet(r => r.AngularVelocity).Returns(4);
        rotatable.SetupGet(r => r.DirectionsCount).Returns(4);

        var cmd = new ChangeVelocityCommand(movable.Object, rotatable.Object);

        cmd.Execute();

        Assert.Equal(new Vector(7, 2), movable.Object.Velocity);
    }
}
