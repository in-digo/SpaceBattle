namespace SpaceBattle.Lib;

public class CheckFuelCommand : ICommand
{
    private readonly IFuelable _fuelable;

    public CheckFuelCommand(IFuelable fuelable)
    {
        _fuelable = fuelable;
    }

    public void Execute()
    {
        if (_fuelable.FuelReserve < _fuelable.FuelConsumption)
            throw new CommandException("Недостаточно топлива");
    }
}