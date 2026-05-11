namespace SpaceBattle.Lib;

public class RetryCommand : ICommand
{
    private readonly ICommand _cmd;

    public RetryCommand(ICommand cmd)
    {
        _cmd = cmd;
    }

    public void Execute()
    {
        _cmd.Execute();
    }
}