namespace SpaceBattle.Lib;

public class Neighborhood
{
    public ISet<IUObject> Objects { get; } = new HashSet<IUObject>();
}