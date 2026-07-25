namespace SpaceBattle.Auth.Server.Games;

public class GameAccessService
{
    private readonly IGameRepository _gameRepository;

    public GameAccessService(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public GameAccessResult CheckAccess(Guid gameId, string userId)
    {
        var game = _gameRepository.GetById(gameId);

        if (game is null)
            return GameAccessResult.GameNotFound;

        return game.ParticipantIds.Contains(userId)
            ? GameAccessResult.Granted
            : GameAccessResult.UserNotParticipant;
    }
}