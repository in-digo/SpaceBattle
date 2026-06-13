namespace SpaceBattle.Lib;

public class RegisterProcessorCommand : ICommand
{
    public void Execute()
    {
        IoC.Resolve<ICommand>(
            "IoC.Register",
            "Server.CommandProcessor",
            (Func<object[], object>)(args =>
            {
                IDictionary<string, object> context = new Dictionary<string, object>();
                new InitProcessorContextCommand(context).Execute();
                context["processor"] = new Processor(new Processable(context));
                return context;
            })
        ).Execute();
    }
}