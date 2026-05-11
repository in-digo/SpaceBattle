namespace SpaceBattle.Lib;

public class BurnFuelCommand : ICommand
{
    private readonly IFuelable _fuelable;

    public BurnFuelCommand(IFuelable fuelable)
    {
        _fuelable = fuelable;
    }

    public void Execute()
    {
        _fuelable.FuelReserve -= _fuelable.FuelConsumption;
    }
}