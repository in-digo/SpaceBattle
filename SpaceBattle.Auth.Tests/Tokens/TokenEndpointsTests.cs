using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SpaceBattle.Auth.Server.Games;
using SpaceBattle.Auth.Server.Tokens;
using SpaceBattle.Auth.Tests.Infrastructure;

namespace SpaceBattle.Auth.Tests.Tokens;

public class TokenEndpointsTests : IClassFixture<AuthWebApplicationFactory>
{
    private readonly AuthWebApplicationFactory _factory;

    public TokenEndpointsTests(AuthWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // Участник игры получает подписанный JWT для созданной игры
    [Fact]
    public async Task CreateToken_UserIsParticipant_ReturnsValidToken()
    {
        // Arrange
        var client = _factory.CreateClient();

        var createGameResponse = await client.PostAsJsonAsync(
            "/games",
            new CreateGameRequest
            {
                ParticipantIds = new[]
                {
                    "user-1",
                    "user-2",
                    "user-3"
                }
            });

        Assert.Equal(
            HttpStatusCode.Created,
            createGameResponse.StatusCode);

        var createdGame =
            await createGameResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(createdGame);

        var tokenRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/games/{createdGame.GameId}/tokens");

        tokenRequest.Headers.Add("X-User-Id", "user-2");

        // Act
        var response = await client.SendAsync(tokenRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.NotNull(tokenResponse);
        Assert.False(string.IsNullOrWhiteSpace(tokenResponse.AccessToken));

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                AuthWebApplicationFactory.SIGNING_KEY));

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidIssuer = AuthWebApplicationFactory.ISSUER,
            ValidateAudience = true,
            ValidAudience = AuthWebApplicationFactory.AUDIENCE,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.Zero,
            ValidAlgorithms = new[]
            {
                SecurityAlgorithms.HmacSha256
            }
        };

        var tokenHandler = new JsonWebTokenHandler();

        var validationResult = await tokenHandler.ValidateTokenAsync(
            tokenResponse.AccessToken,
            validationParameters);

        Assert.True(
            validationResult.IsValid,
            validationResult.Exception?.Message);

        Assert.Equal(
            "user-2",
            validationResult.ClaimsIdentity.FindFirst("sub")?.Value);

        Assert.Equal(
            createdGame.GameId.ToString(),
            validationResult.ClaimsIdentity
                .FindFirst("game_id")?.Value);
    }

    // Запрос токена без идентификатора пользователя возвращает HTTP 401
    [Fact]
    public async Task CreateToken_UserIdHeaderIsMissing_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        var createGameResponse = await client.PostAsJsonAsync(
            "/games",
            new CreateGameRequest
            {
                ParticipantIds = new[]
                {
                    "user-1",
                    "user-2"
                }
            });

        Assert.Equal(
            HttpStatusCode.Created,
            createGameResponse.StatusCode);

        var createdGame =
            await createGameResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(createdGame);

        // Act
        var response = await client.PostAsync(
            $"/games/{createdGame.GameId}/tokens",
            content: null);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    // Пользователь вне списка участников получает HTTP 403
    [Fact]
    public async Task CreateToken_UserIsNotParticipant_ReturnsForbidden()
    {
        // Arrange
        var client = _factory.CreateClient();

        var createGameResponse = await client.PostAsJsonAsync(
            "/games",
            new CreateGameRequest
            {
                ParticipantIds = new[]
                {
                    "user-1",
                    "user-2"
                }
            });

        Assert.Equal(
            HttpStatusCode.Created,
            createGameResponse.StatusCode);

        var createdGame =
            await createGameResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(createdGame);

        var tokenRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/games/{createdGame.GameId}/tokens");

        tokenRequest.Headers.Add("X-User-Id", "user-99");

        // Act
        var response = await client.SendAsync(tokenRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    // Запрос токена для неизвестной игры возвращает HTTP 404
    [Fact]
    public async Task CreateToken_GameDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();
        var unknownGameId = Guid.NewGuid();

        var tokenRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/games/{unknownGameId}/tokens");

        tokenRequest.Headers.Add("X-User-Id", "user-1");

        // Act
        var response = await client.SendAsync(tokenRequest);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }
}