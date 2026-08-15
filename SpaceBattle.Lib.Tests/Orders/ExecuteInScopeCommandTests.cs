namespace SpaceBattle.Lib.Tests;

public class ExecuteInScopeCommandTests : IDisposable
{
    public ExecuteInScopeCommandTests()
    {
        new InitScopeBasedIoCCommand().Execute();

        IoC.Resolve<ICommand>(
            "Scopes.Current",
            IoC.Resolve<object>("IoC.Scope.Create")
        ).Execute();
    }

    public void Dispose()
    {
        IoC.Resolve<ICommand>("Scopes.Current.Clear").Execute();
    }

    // Проверяет, что команда выполняется в переданном IoC-скоупе, а после успешного выполнения текущим снова становится прежний скоуп
    [Fact]
    public void Execute_UsesSpecifiedScopeAndRestoresPreviousScope()
    {
        // Arrange
        var previousScope = IoC.Resolve<object>("IoC.Scope.Current");

        IoC.Resolve<ICommand>(
            "IoC.Register",
            "Scope.Marker",
            (Func<object[], object>)(_ => "previous")
        ).Execute();

        var playerScope = IoC.Resolve<object>("IoC.Scope.Create");

        IoC.Resolve<ICommand>(
            "Scopes.Current",
            playerScope
        ).Execute();

        IoC.Resolve<ICommand>(
            "IoC.Register",
            "Scope.Marker",
            (Func<object[], object>)(_ => "player")
        ).Execute();

        IoC.Resolve<ICommand>(
            "Scopes.Current",
            previousScope
        ).Execute();

        string? markerResolvedInsideCommand = null;

        var innerCommand = new ActionCommand(() =>
        {
            markerResolvedInsideCommand = IoC.Resolve<string>("Scope.Marker");
        });

        var command = new ExecuteInScopeCommand(playerScope, innerCommand);

        // Act
        command.Execute();

        // Assert
        Assert.Equal(
            "player",
            markerResolvedInsideCommand);

        Assert.Same(
            previousScope,
            IoC.Resolve<object>("IoC.Scope.Current"));

        Assert.Equal(
            "previous",
            IoC.Resolve<string>("Scope.Marker"));
    }

    // Проверяет, что при исключении во вложенной команде исходное исключение передаётся выше,
    // а текущим снова становится скоуп, который был активен до выполнения команды
    [Fact]
    public void Execute_RestoresPreviousScope_WhenInnerCommandThrows()
    {
        // Arrange
        var previousScope = IoC.Resolve<object>("IoC.Scope.Current");
        var playerScope = IoC.Resolve<object>("IoC.Scope.Create");

        var expectedException = new InvalidOperationException("Command execution failed.");

        var innerCommand = new ActionCommand(() => throw expectedException);

        var command = new ExecuteInScopeCommand(playerScope, innerCommand);

        // Act
        var actualException = Assert.Throws<InvalidOperationException>(() => command.Execute());

        // Assert
        Assert.Same(
            expectedException,
            actualException);

        Assert.Same(
            previousScope,
            IoC.Resolve<object>("IoC.Scope.Current"));
    }
}