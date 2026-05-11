namespace SpaceBattle.Lib;

public class LogCommand : ICommand
{
    private readonly Exception _ex;
    private readonly ILog _log;

    public LogCommand(Exception ex, ILog log)
    {
        _ex = ex;
        _log = log;
    }

    public void Execute()
    {
        _log.Write(_ex);
    }
}