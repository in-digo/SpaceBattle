using Moq;

namespace SpaceBattle.Lib.Tests;

public class BurnFuelCommandTests
{
    //Расход топива
    [Fact]
    public void BurnFuel_DecreasesFuel()
    {
        var fuelable = new Mock<IFuelable>();
        fuelable.SetupGet(f => f.FuelReserve).Returns(15);
        fuelable.SetupGet(f => f.FuelConsumption).Returns(5);

        var cmd = new BurnFuelCommand(fuelable.Object);

        cmd.Execute();

        fuelable.VerifySet(f => f.FuelReserve = 10, Times.Once);
    }

    //Расход топива, остаётся 0
    [Fact]
    public void BurnFuel_LeavesZero()
    {
        var fuelable = new Mock<IFuelable>();
        fuelable.SetupGet(f => f.FuelReserve).Returns(5);
        fuelable.SetupGet(f => f.FuelConsumption).Returns(5);

        var cmd = new BurnFuelCommand(fuelable.Object);

        cmd.Execute();

        fuelable.VerifySet(f => f.FuelReserve = 0, Times.Once);
    }
}