namespace SpaceBattle.Auth.Server.Games;

public class CreateGameRequest
{
    public string[]? ParticipantIds { get; set; } = Array.Empty<string>();
}