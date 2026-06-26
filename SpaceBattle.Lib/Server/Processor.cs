namespace SpaceBattle.Lib;

public class Processor
{
    Thread _thread;
    IProcessable _processable;

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