using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace SpaceBattle.Lib.Tests;

public class GameServerAuthorizationTests : IClassFixture<GameServerWebApplicationFactory>
{
    private readonly GameServerWebApplicationFactory _factory;

    public GameServerAuthorizationTests(GameServerWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // Игровой сервер отклоняет входящее сообщение без JWT
    [Fact]
    public async Task SendMessage_TokenIsMissing_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        var message = new
        {
            gameId = Guid.NewGuid().ToString(),
            objectId = "548",
            operationId = "move.forward",
            args = new
            {
                velocity = 2
            }
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/game/message",
            message);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    // Игровой сервер отклоняет JWT, подписанный неизвестным ключом
    [Fact]
    public async Task SendMessage_TokenHasInvalidSignature_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var gameId = Guid.NewGuid();

        var token = CreateToken(
            signingKey: "another-signing-key-with-at-least-32-characters",
            gameId: gameId,
            userId: "user-1");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var message = new
        {
            gameId = gameId.ToString(),
            objectId = "548",
            operationId = "move.forward",
            args = new
            {
                velocity = 2
            }
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/game/message",
            message);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    // Игровой сервер отклоняет JWT с истёкшим сроком действия
    [Fact]
    public async Task SendMessage_TokenIsExpired_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var gameId = Guid.NewGuid();
        var currentTime = DateTimeOffset.UtcNow;

        var token = CreateToken(
            signingKey:  GameServerWebApplicationFactory.SIGNING_KEY,
            gameId: gameId,
            userId: "user-1",
            issuedAtUtc: currentTime.AddMinutes(-20),
            expiresAtUtc: currentTime.AddMinutes(-10));

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var message = new
        {
            gameId = gameId.ToString(),
            objectId = "548",
            operationId = "move.forward",
            args = new
            {
                velocity = 2
            }
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/game/message",
            message);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    // Игровой сервер запрещает использовать JWT для управления другой игрой
    [Fact]
    public async Task SendMessage_TokenBelongsToAnotherGame_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();

        var tokenGameId = Guid.NewGuid();
        var messageGameId = Guid.NewGuid();

        var token = CreateToken(
            signingKey: GameServerWebApplicationFactory.SIGNING_KEY,
            gameId: tokenGameId,
            userId: "user-1");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var message = new
        {
            gameId = messageGameId.ToString(),
            objectId = "548",
            operationId = "move.forward",
            args = new
            {
                velocity = 2
            }
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/game/message",
            message);

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    // Игровой сервер принимает сообщение с JWT, выданным для этой игры
    [Fact]
    public async Task SendMessage_TokenBelongsToMessageGame_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var gameId = Guid.NewGuid();

        _factory.MessageEndpoint.Reset();

        var token = CreateToken(
            signingKey: GameServerWebApplicationFactory.SIGNING_KEY,
            gameId: gameId,
            userId: "user-1");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var message = new
        {
            gameId = gameId.ToString(),
            objectId = "548",
            operationId = "move.forward",
            args = new
            {
                velocity = 2
            }
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/game/message",
            message);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.NotNull(
            _factory.MessageEndpoint.HandledJson);
    }

    // Игровой сервер отклоняет JWT с неизвестным issuer или audience
    [Theory]
    [InlineData("Unknown.Auth.Server", "SpaceBattle.Game.Server")]
    [InlineData("SpaceBattle.Auth.Server", "Unknown.Game.Server")]
    public async Task SendMessage_TokenHasInvalidIssuerOrAudience_ReturnsUnauthorized(
        string issuer,
        string audience)
    {
        // Arrange
        var client = _factory.CreateClient();
        var gameId = Guid.NewGuid();

        var token = CreateToken(
            signingKey: GameServerWebApplicationFactory.SIGNING_KEY,
            gameId: gameId,
            userId: "user-1",
            issuer: issuer,
            audience: audience);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var message = new
        {
            gameId = gameId.ToString(),
            objectId = "548",
            operationId = "move.forward",
            args = new
            {
                velocity = 2
            }
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/game/message",
            message);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    private static string CreateToken(
        string signingKey,
        Guid gameId,
        string userId,
        DateTimeOffset? issuedAtUtc = null,
        DateTimeOffset? expiresAtUtc = null,
        string? issuer = null,
        string? audience = null)
    {
        var issuedAt = issuedAtUtc ?? DateTimeOffset.UtcNow;
        var expiresAt = expiresAtUtc ?? issuedAt.AddMinutes(15);

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(signingKey));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer ?? GameServerWebApplicationFactory.ISSUER,

            Audience = audience ?? GameServerWebApplicationFactory.AUDIENCE,

            IssuedAt = issuedAt.UtcDateTime,
            Expires = expiresAt.UtcDateTime,

            Claims = new Dictionary<string, object>
            {
                ["sub"] = userId,
                ["game_id"] = gameId.ToString(),
                ["jti"] = Guid.NewGuid().ToString()
            },

            SigningCredentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler()
            .CreateToken(tokenDescriptor);
    }
}