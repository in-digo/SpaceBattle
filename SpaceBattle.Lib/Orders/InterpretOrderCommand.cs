namespace SpaceBattle.Lib;

public class InterpretOrderCommand : ICommand
{
    private readonly IUObject _order;

    public InterpretOrderCommand(IUObject order)
    {
        _order = order;
    }

    public void Execute()
    {
        var action = (string)_order.GetProperty("action");

        IoC.Resolve<ICommand>(
            $"Commands.{action}",
            _order
        ).Execute();
    }
}