namespace SpaceBattle.Lib;

public class UpdateNeighborhoodCommand : ICommand
{
    private readonly IUObject _gameObject;
    private readonly INeighborhoodSystem _neighborhoodSystem;
    private readonly ICollisionCommandFactory _collisionCommandFactory;
    private readonly ICommand _nextCommand;

    private Neighborhood? _currentNeighborhood;
    private MacroCommand? _collisionMacroCommand;

    public UpdateNeighborhoodCommand(
        IUObject gameObject,
        INeighborhoodSystem neighborhoodSystem,
        ICollisionCommandFactory collisionCommandFactory,
        ICommand nextCommand)
    {
        _gameObject = gameObject;
        _neighborhoodSystem = neighborhoodSystem;
        _collisionCommandFactory = collisionCommandFactory;
        _nextCommand = nextCommand;
    }

    public void Execute()
    {
        var newNeighborhood = _neighborhoodSystem.GetNeighborhood(_gameObject);

        if (ReferenceEquals(_currentNeighborhood, newNeighborhood))
        {
            _collisionMacroCommand?.Execute();
        }
        else
        {
            _currentNeighborhood?.Objects.Remove(_gameObject);
            newNeighborhood.Objects.Add(_gameObject);

            var collisionCommands = newNeighborhood.Objects
                .Where(otherObject => !ReferenceEquals(otherObject, _gameObject))
                .Select(otherObject => _collisionCommandFactory.Create(_gameObject, otherObject))
                .ToArray();

            _collisionMacroCommand = new MacroCommand(collisionCommands);
            _currentNeighborhood = newNeighborhood;
        }

        _nextCommand.Execute();
    }
}