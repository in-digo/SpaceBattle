namespace SpaceBattle.Lib;

public class GridNeighborhoodSystem : INeighborhoodSystem
{
    private readonly int _neighborhoodSize;
    private readonly Vector _offset;

    private readonly IDictionary<Vector, Neighborhood> _neighborhoods  = new Dictionary<Vector, Neighborhood>();

    public GridNeighborhoodSystem(int neighborhoodSize, Vector offset)
    {
        if (neighborhoodSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(neighborhoodSize), "Размер окрестности должен быть больше нуля");

        _neighborhoodSize = neighborhoodSize;
        _offset = offset;
    }

    public Neighborhood GetNeighborhood(IUObject gameObject)
    {
        var position = (Vector)gameObject.GetProperty("Position");
        if (position.Coords.Length != _offset.Coords.Length)
        {
            throw new ArgumentException(
                "Размерность позиции объекта должна совпадать с размерностью смещения сетки",
                nameof(gameObject));
        }

        var cellCoords = new int[position.Coords.Length];

        for (var i = 0; i < position.Coords.Length; i++)
            cellCoords[i] = (int)Math.Floor((double)(position.Coords[i] - _offset.Coords[i]) / _neighborhoodSize);

        var cell = new Vector(cellCoords);

        if (!_neighborhoods.TryGetValue(cell, out var neighborhood))
        {
            neighborhood = new Neighborhood();
            _neighborhoods.Add(cell, neighborhood);
        }

        return neighborhood;
    }
}