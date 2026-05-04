namespace SpaceBattle.Lib;

public interface IRotatable
{
    int Direction { get; set; }
    int AngularVelocity { get; }
    int DirectionsCount { get; }
}