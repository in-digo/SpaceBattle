namespace SpaceBattle.Lib;

public class RotateCommand
{
    private readonly IRotatable _rotatable;

    public RotateCommand(IRotatable rotatable)
    {
        _rotatable = rotatable;
    }

    public void Execute()
    {
        _rotatable.Direction = (_rotatable.Direction + _rotatable.AngularVelocity) % _rotatable.DirectionsCount;
    }
}