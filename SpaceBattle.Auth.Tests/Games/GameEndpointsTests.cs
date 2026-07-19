using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SpaceBattle.Auth.Server.Games;

namespace SpaceBattle.Auth.Tests.Games;

public class GameEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public GameEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    // Создание игры возвращает HTTP 201 и новый идентификатор
    [Fact]
    public async Task CreateGame_ValidParticipants_ReturnsCreatedWithGameId()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateGameRequest
        {
            ParticipantIds = new[]
            {
                "user-1",
                "user-2",
                "user-3"
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/games", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var responseBody = await response.Content.ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(responseBody);
        Assert.NotEqual(Guid.Empty, responseBody.GameId);
    }

    // Создание игры без участников возвращает HTTP 400
    [Fact]
    public async Task CreateGame_EmptyParticipants_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateGameRequest
        {
            ParticipantIds = Array.Empty<string>()
        };

        // Act
        var response = await client.PostAsJsonAsync("/games", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // Создание игры с null вместо списка участников возвращает HTTP 400
    [Fact]
    public async Task CreateGame_NullParticipants_ReturnsBadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new
        {
            participantIds = (string[]?)null
        };

        // Act
        var response = await client.PostAsJsonAsync("/games", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}