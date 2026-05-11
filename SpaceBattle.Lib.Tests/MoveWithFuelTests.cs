using Moq;

namespace SpaceBattle.Lib.Tests;

public class MoveWithFuelTests
{
    //Топлива достаточно - объект перемещается, топливо уменьшается
    [Fact]
    public void EnoughFuel_MovesAndBurnsFuel()
    {
        var fuelable = new Mock<IFuelable>();
        fuelable.SetupProperty(f => f.FuelReserve, 15);
        fuelable.SetupGet(f => f.FuelConsumption).Returns(5);

        var movable = new Mock<IMovable>();
        movable.SetupGet(m => m.Position).Returns(new Vector(12, 5));
        movable.SetupGet(m => m.Velocity).Returns(new Vector(-7, 3));

        var macroCmd = new MacroCommand(
            new CheckFuelCommand(fuelable.Object),
            new MoveCommand(movable.Object),
            new BurnFuelCommand(fuelable.Object));

        macroCmd.Execute();

        movable.VerifySet(m => m.Position = new Vector(5, 8), Times.Once);
        Assert.Equal(10, fuelable.Object.FuelReserve);
    }

    //Топлива недостаточно - CommandException, объект не перемещается, топливо не уменьшается
    [Fact]
    public void NotEnoughFuel_NoMovesAndBurnsFuel()
    {
        var fuelable = new Mock<IFuelable>();
        fuelable.SetupProperty(f => f.FuelReserve, 2);
        fuelable.SetupGet(f => f.FuelConsumption).Returns(5);

        var movable = new Mock<IMovable>();

        var macroCmd = new MacroCommand(
            new CheckFuelCommand(fuelable.Object),
            new MoveCommand(movable.Object),
            new BurnFuelCommand(fuelable.Object));

        Assert.Throws<CommandException>(() => macroCmd.Execute());
        movable.VerifySet(m => m.Position = It.IsAny<Vector>(), Times.Never);
        Assert.Equal(2, fuelable.Object.FuelReserve);
    }
}