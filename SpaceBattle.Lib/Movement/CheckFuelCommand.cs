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
        
    }
}