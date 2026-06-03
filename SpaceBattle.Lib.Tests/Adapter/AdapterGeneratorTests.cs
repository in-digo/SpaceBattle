namespace SpaceBattle.Lib.Tests;

public interface ITestMovable
{
    Vector GetPosition();
    void SetPosition(Vector newValue);
    Vector GetVelocity();
}

public class AdapterGeneratorTests
{
    // Генератор возвращает тип, реализующий переданный интерфейс
    [Fact]
    public void Generate_ReturnsTypeThatImplementsInterface()
    {
        var adapterType = AdapterGenerator.Generate(typeof(ITestMovable));

        Assert.True(typeof(ITestMovable).IsAssignableFrom(adapterType));
    }
}