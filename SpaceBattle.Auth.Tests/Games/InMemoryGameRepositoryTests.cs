using SpaceBattle.Auth.Server.Games;

namespace SpaceBattle.Auth.Tests.Games;

public class InMemoryGameRepositoryTests
{
    // Добавленную игру можно получить из хранилища по её идентификатору
    [Fact]
    public void Add_GameCanBeRetrievedById()
    {
        // Arrange
        var repository = new InMemoryGameRepository();
        var game = new Game(
            Guid.NewGuid(),
            new[] { "user-1", "user-2", "user-3" });

        // Act
        repository.Add(game);
        var actualGame = repository.GetById(game.Id);

        // Assert
        Assert.Same(game, actualGame);
    }

    // При запросе неизвестного идентификатора хранилище возвращает null
    [Fact]
    public void GetById_UnknownGame_ReturnsNull()
    {
        // Arrange
        var repository = new InMemoryGameRepository();
        var unknownGameId = Guid.NewGuid();

        // Act
        var game = repository.GetById(unknownGameId);

        // Assert
        Assert.Null(game);
    }
}