namespace SpaceBattle.Auth.Server.Games;

public interface IGameRepository
{
    void Add(Game game);

    Game? GetById(Guid gameId);
}