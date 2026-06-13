namespace SpaceBattle.Lib;

// Регистрирует в текущем скоупе стратегию "Adapter":
// IoC.Resolve<T>("Adapter", typeof(T), obj) создаёт адаптер для obj
public class RegisterAdapterStrategyCommand : ICommand
{
    public void Execute()
    {
        IoC.Resolve<ICommand>(
            "IoC.Register",
            "Adapter",
            (Func<object[], object>)(args =>
            {
                var interfaceType = (Type)args[0];
                var obj = args[1];
                var adapterType = AdapterGenerator.Generate(interfaceType);
                return Activator.CreateInstance(adapterType, obj)!;
            })
        ).Execute();
    }
}