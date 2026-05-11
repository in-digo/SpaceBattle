namespace SpaceBattle.Lib;

public class ChangeVelocityCommand : ICommand
{
    private readonly IMovable _movable;
    private readonly IRotatable _rotatable;

    public ChangeVelocityCommand(IMovable movable, IRotatable rotatable)
    {
        _movable = movable;
        _rotatable = rotatable;
    }

    public void Execute()
    {
        var velocity = _movable.Velocity;
        double angle = 2 * Math.PI * _rotatable.AngularVelocity / _rotatable.DirectionsCount;

        double cos = Math.Cos(angle);
        double sin = Math.Sin(angle);

        var coords = (int[])velocity.Coords.Clone();
        coords[0] = (int)Math.Round(velocity.Coords[0] * cos - velocity.Coords[1] * sin);
        coords[1] = (int)Math.Round(velocity.Coords[0] * sin + velocity.Coords[1] * cos);

        _movable.Velocity = new Vector(coords);
    }
}