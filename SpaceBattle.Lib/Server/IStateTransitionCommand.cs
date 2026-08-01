namespace SpaceBattle.Lib;

public interface IStateTransitionCommand : ICommand
{
    ICommandProcessingState? NextState { get; }
}