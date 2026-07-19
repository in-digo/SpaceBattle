using SpaceBattle.Auth.Server.Games;

namespace SpaceBattle.Auth.Tests.Games;

public class GameAccessServiceTests
{
    // Пользователь из списка участников получает доступ к игре
    [Fact]
    public void CheckAccess_UserIsParticipant_ReturnsGranted()
    {
        // Arrange
        var repository = new InMemoryGameRepository();
        var game = new Game(
            Guid.NewGuid(),
            new[] { "user-1", "user-2", "user-3" });

        repository.Add(game);

        var service = new GameAccessService(repository);

        // Act
        var result = service.CheckAccess(game.Id, "user-2");

        // Assert
        Assert.Equal(GameAccessResult.Granted, result);
    }

    // Пользователь вне списка участников не получает доступ к существующей игре
    [Fact]
    public void CheckAccess_UserIsNotParticipant_ReturnsUserNotParticipant()
    {
        // Arrange
        var repository = new InMemoryGameRepository();
        var game = new Game(
            Guid.NewGuid(),
            new[] { "user-1", "user-2", "user-3" });

        repository.Add(game);

        var service = new GameAccessService(repository);

        // Act
        var result = service.CheckAccess(game.Id, "user-99");

        // Assert
        Assert.Equal(GameAccessResult.UserNotParticipant, result);
    }

    // Проверка доступа к неизвестной игре возвращает результат GameNotFound
    [Fact]
    public void CheckAccess_GameDoesNotExist_ReturnsGameNotFound()
    {
        // Arrange
        var repository = new InMemoryGameRepository();
        var service = new GameAccessService(repository);
        var unknownGameId = Guid.NewGuid();

        // Act
        var result = service.CheckAccess(unknownGameId, "user-1");

        // Assert
        Assert.Equal(GameAccessResult.GameNotFound, result);
    }
}