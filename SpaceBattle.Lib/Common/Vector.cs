namespace SpaceBattle.Lib;

public class Vector
{
    public int X { get; }
    public int Y { get; }

    public Vector(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static Vector operator +(Vector a, Vector b)
    {
        return new Vector(a.X + b.X, a.Y + b.Y);
    }

    public override bool Equals(object? obj)
    {
        if (obj is Vector other)
            return X == other.X && Y == other.Y;
        return false;
    }

    public override int GetHashCode() => HashCode.Combine(X, Y);
}