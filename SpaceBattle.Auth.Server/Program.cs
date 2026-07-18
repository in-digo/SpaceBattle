using SpaceBattle.Auth.Server.Games;

var builder = WebApplication.CreateBuilder(args);

// Хранилище должно сохранять игры между отдельными HTTP-запросами
builder.Services.AddSingleton<IGameRepository, InMemoryGameRepository>();
builder.Services.AddSingleton<GameService>();

var app = builder.Build();

// Endpoint позволяет внешним системам проверить доступность сервиса
app.MapGet("/health", () => Results.Ok());

app.MapPost("/games", (
    CreateGameRequest request,
    GameService gameService) =>
{
    if (request.ParticipantIds is null || request.ParticipantIds.Length == 0)
        return Results.BadRequest();

    var gameId = gameService.CreateGame(request.ParticipantIds);
    var response = new CreateGameResponse(gameId);

    return Results.Created($"/games/{gameId}", response);
});

app.Run();

public partial class Program
{
}