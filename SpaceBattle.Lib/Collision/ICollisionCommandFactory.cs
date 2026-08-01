namespace SpaceBattle.Lib;

public interface ICollisionCommandFactory
{
    ICommand Create(IUObject firstObject, IUObject secondObject);
}