namespace SpaceBattle.Lib;

public interface IMessageEndpoint
{
    void Handle(string json);
}