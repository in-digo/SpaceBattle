using SpaceBattle.Auth.Server.Games;

namespace SpaceBattle.Auth.Tests.Games;

public class GameServiceTests
{
    // Создание игры возвращает новый идентификатор и сохраняет всех участников
    [Fact]
    public void CreateGame_ValidParticipants_CreatesAndStoresGame()
    {
        // Arrange
        var repository = new InMemoryGameRepository();
        var service = new GameService(repository);
        var participantIds = new[]
        {
            "user-1",
            "user-2",
            "user-3"
        };

        // Act
        var gameId = service.CreateGame(participantIds);

        // Assert
        Assert.NotEqual(Guid.Empty, gameId);

        var savedGame = repository.GetById(gameId);

        Assert.NotNull(savedGame);
        Assert.Equal(participantIds, savedGame.ParticipantIds.ToArray());
    }

    // Создание игры с пустым списком участников запрещено
    [Fact]
    public void CreateGame_EmptyParticipants_ThrowsArgumentException()
    {
        // Arrange
        var repository = new InMemoryGameRepository();
        var service = new GameService(repository);
        var participantIds = Array.Empty<string>();

        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => service.CreateGame(participantIds));

        // Assert
        Assert.Equal("participantIds", exception.ParamName);
    }
}