namespace SpaceBattle.Auth.Server.Games;

public class Game
{
    public Game(Guid id, IReadOnlyCollection<string> participantIds)
    {
        Id = id;
        ParticipantIds = participantIds;
    }

    public Guid Id { get; }

    public IReadOnlyCollection<string> ParticipantIds { get; }
}