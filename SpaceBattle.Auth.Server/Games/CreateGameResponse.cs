namespace SpaceBattle.Auth.Server.Games;

public class CreateGameResponse
{
    public Guid GameId { get; }
    
    public CreateGameResponse(Guid gameId)
    {
        GameId = gameId;
    }
}