namespace SpaceBattle.Lib;

public class RegisterPlayerObjectsCommand : ICommand
{
    private readonly IReadOnlyDictionary<string, IUObject> _objects;

    public RegisterPlayerObjectsCommand(IReadOnlyDictionary<string, IUObject> objects)
    {
        _objects = objects;
    }

    public void Execute()
    {
        IoC.Resolve<ICommand>(
            "IoC.Register",
            "Game.Objects.Get",
            (Func<object[], object>)(args =>
            {
                var objectId = (string)args[0];

                if (!_objects.TryGetValue(objectId, out var gameObject))
                    throw new UnauthorizedAccessException($"Access to object '{objectId}' is denied");

                return gameObject;
            })
        ).Execute();
    }
}