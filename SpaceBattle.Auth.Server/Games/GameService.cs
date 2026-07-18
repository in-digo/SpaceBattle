namespace SpaceBattle.Auth.Server.Games;

public class GameService
{
    private readonly IGameRepository _gameRepository;

    public GameService(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public Guid CreateGame(IReadOnlyCollection<string> participantIds)
    {
        if (participantIds.Count == 0)
        {
            throw new ArgumentException(
                "Игра должна содержать хотя бы одного участника",
                nameof(participantIds));
        }

        // Идентификатор создаётся сервером, чтобы клиент не мог выбрать его самостоятельно
        var game = new Game(Guid.NewGuid(), participantIds);

        _gameRepository.Add(game);

        return game.Id;
    }
}