namespace SpaceBattle.Lib;

public static class ExceptionHandler
{
    private static Dictionary<Type, Dictionary<Type, Func<ICommand, Exception, ICommand>>> _store = new();
    private static Func<ICommand, Exception, ICommand> _default = (cmd, ex) => throw ex;

    public static void Setup()
    {
        _store = new();
        _default = (cmd, ex) => throw ex;
    }

    public static void SetDefault(Func<ICommand, Exception, ICommand> handler)
    {
        _default = handler;
    }

    public static void Register(Type cmdType, Type exType, Func<ICommand, Exception, ICommand> handler)
    {
        if (!_store.ContainsKey(cmdType))
            _store[cmdType] = new();

        _store[cmdType][exType] = handler;
    }

    public static ICommand Handle(ICommand cmd, Exception ex)
    {
        Type cmdType = cmd.GetType();
        Type exType = ex.GetType();

        if (_store.TryGetValue(cmdType, out var inner) && inner.TryGetValue(exType, out var handler))
            return handler(cmd, ex);

        return _default(cmd, ex);
    }
}