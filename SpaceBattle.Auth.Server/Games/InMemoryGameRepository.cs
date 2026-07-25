using System.Collections.Concurrent;

namespace SpaceBattle.Auth.Server.Games;

public class InMemoryGameRepository : IGameRepository
{
    // Потокобезопасная коллекция позволяет одновременно обрабатывать несколько HTTP-запросов
    private readonly ConcurrentDictionary<Guid, Game> _games = new();

    public void Add(Game game)
    {
        _games[game.Id] = game;
    }

    public Game? GetById(Guid gameId)
    {
        _games.TryGetValue(gameId, out var game);

        return game;
    }
}