using SpaceBattle.Auth.Server.Games;
using SpaceBattle.Auth.Server.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Хранилище должно сохранять игры между отдельными HTTP-запросами
builder.Services.AddSingleton<IGameRepository, InMemoryGameRepository>();
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<GameAccessService>();

// Options связывается после подключения всех источников конфигурации
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));
    
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

var app = builder.Build();

// Endpoint позволяет внешним системам проверить доступность сервиса
app.MapGet("/health", () => Results.Ok());

app.MapPost("/games", (
    CreateGameRequest request,
    GameService gameService) =>
{
    // HTTP-слой преобразует некорректные входные данные в клиентскую ошибку
    if (request.ParticipantIds is null ||
        request.ParticipantIds.Length == 0)
    {
        return Results.BadRequest();
    }

    var gameId = gameService.CreateGame(request.ParticipantIds);
    var response = new CreateGameResponse(gameId);

    return Results.Created($"/games/{gameId}", response);
});

app.MapPost("/games/{gameId:guid}/tokens", (
    Guid gameId,
    HttpContext context,
    GameAccessService gameAccessService,
    ITokenService tokenService) =>
{
    if (!context.Request.Headers.TryGetValue(
            "X-User-Id",
            out var userIdHeader) ||
        string.IsNullOrWhiteSpace(userIdHeader.ToString()))
    {
        return Results.Unauthorized();
    }

    // Идентификатор считается проверенным внешней системой аутентификации
    var userId = userIdHeader.ToString();

    var accessResult = gameAccessService.CheckAccess(gameId, userId);

    switch (accessResult)
    {
        case GameAccessResult.GameNotFound:
            return Results.NotFound();

        case GameAccessResult.UserNotParticipant:
            return Results.StatusCode(
                StatusCodes.Status403Forbidden);

        case GameAccessResult.Granted:
            break;

        default:
            throw new InvalidOperationException(
                $"Неизвестный результат проверки доступа: {accessResult}.");
    }

    var tokenResult = tokenService.CreateToken(gameId, userId);

    var response = new TokenResponse(
        tokenResult.AccessToken,
        tokenResult.ExpiresAtUtc);

    return Results.Ok(response);
});

app.Run();

public partial class Program
{
}