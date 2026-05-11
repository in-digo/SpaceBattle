using Moq;

namespace SpaceBattle.Lib.Tests;

public class CheckFuelCommandTests
{
    //Хватает топлива - без исключения
    [Fact]
    public void EnoughFuel_NoException()
    {
        var fuelable = new Mock<IFuelable>();
        fuelable.SetupGet(f => f.FuelReserve).Returns(10);
        fuelable.SetupGet(f => f.FuelConsumption).Returns(5);

        var cmd = new CheckFuelCommand(fuelable.Object);

        cmd.Execute();
    }

    //Ровно хватает топлива - без исключения
    [Fact]
    public void FuelExactlyEqualsConsumption_NoException()
    {
        var fuelable = new Mock<IFuelable>();
        fuelable.SetupGet(f => f.FuelReserve).Returns(5);
        fuelable.SetupGet(f => f.FuelConsumption).Returns(5);

        var cmd = new CheckFuelCommand(fuelable.Object);

        cmd.Execute();
    }

    //Не хватает топлива - CommandException
    [Fact]
    public void NotEnoughFuel_ThrowsCommandException()
    {
        var fuelable = new Mock<IFuelable>();
        fuelable.SetupGet(f => f.FuelReserve).Returns(3);
        fuelable.SetupGet(f => f.FuelConsumption).Returns(5);

        var cmd = new CheckFuelCommand(fuelable.Object);

        Assert.Throws<CommandException>(() => cmd.Execute());
    }
}