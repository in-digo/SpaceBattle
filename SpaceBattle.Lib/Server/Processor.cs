namespace SpaceBattle.Lib;

public class Processor
{
    private readonly Thread _thread;
    private readonly IProcessable _processable;

    public Processor(IProcessable processable)
    {
        _processable = processable;
        _thread = new Thread(Evaluation);
        _thread.Start();
    }

    public bool Wait(int milliseconds) => _thread.Join(milliseconds);

    private void Evaluation()
    {
        try
        {
            while (_processable.CanContinue)
                _processable.Process();
        }
        catch (Exception ex)
        {
            _processable.Terminate(ex);
        }
    }
}