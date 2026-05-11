namespace SpaceBattle.Lib;

public class RetryTwoCommand : ICommand
{
    private readonly ICommand _cmd;

    public RetryTwoCommand(ICommand cmd)
    {
        _cmd = cmd;
    }

    public void Execute()
    {
        _cmd.Execute();
    }
}