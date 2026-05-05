namespace SpaceBattle.Lib;

public class Vector
{
    public int[] Coords { get; }

    public Vector(params int[] coords)
    {
        Coords = coords;
    }

    public static Vector operator +(Vector a, Vector b)
    {
        if (a.Coords.Length != b.Coords.Length)
            throw new ArgumentException("Векторы должны быть одной размерности");

        var result = new int[a.Coords.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = a.Coords[i] + b.Coords[i];

        return new Vector(result);
    }

    public override bool Equals(object? obj)
    {
        if (obj is Vector other)
            return Coords.SequenceEqual(other.Coords);
        return false;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var c in Coords)
            hash.Add(c);
        return hash.ToHashCode();
    }
}