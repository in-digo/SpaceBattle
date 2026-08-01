namespace SpaceBattle.Lib;

public interface ICommandProcessingState
{
    ICommandProcessingState? Handle();
}