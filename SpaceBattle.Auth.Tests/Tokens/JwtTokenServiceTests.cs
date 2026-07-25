using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SpaceBattle.Auth.Server.Tokens;
using Microsoft.Extensions.Options;

namespace SpaceBattle.Auth.Tests.Tokens;

public class JwtTokenServiceTests
{
    private class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _currentTime;

        public FixedTimeProvider(DateTimeOffset currentTime)
        {
            _currentTime = currentTime;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _currentTime;
        }
    }

    private const string SIGNING_KEY =
        "test-signing-key-with-at-least-32-characters";

    // Сервис создаёт подписанный JWT с идентификаторами пользователя и игры
    [Fact]
    public async Task CreateToken_ValidData_ReturnsValidTokenWithRequiredClaims()
    {
        // Arrange
        var currentTime = DateTimeOffset.UtcNow;
        var gameId = Guid.NewGuid();
        const string userId = "user-1";

        var options = new JwtOptions
        {
            Issuer = "SpaceBattle.Auth.Server",
            Audience = "SpaceBattle.Game.Server",
            SigningKey = SIGNING_KEY,
            Lifetime = TimeSpan.FromMinutes(15)
        };

        var timeProvider = new FixedTimeProvider(currentTime);
        var service = new JwtTokenService(
            Options.Create(options),
            timeProvider);

        // Act
        var tokenResult = service.CreateToken(gameId, userId);

        // Assert
        Assert.Equal(
            currentTime.Add(options.Lifetime),
            tokenResult.ExpiresAtUtc);

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(SIGNING_KEY));

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.Zero,
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
        };

        var tokenHandler = new JsonWebTokenHandler();

        var validationResult = await tokenHandler.ValidateTokenAsync(
            tokenResult.AccessToken,
            validationParameters);

        Assert.True(
            validationResult.IsValid,
            validationResult.Exception?.Message);

        Assert.Equal(
            userId,
            validationResult.ClaimsIdentity.FindFirst("sub")?.Value);

        Assert.Equal(
            gameId.ToString(),
            validationResult.ClaimsIdentity.FindFirst("game_id")?.Value);

        Assert.Equal(
            currentTime.ToUnixTimeSeconds().ToString(),
            validationResult.ClaimsIdentity.FindFirst("iat")?.Value);

        Assert.Equal(
            tokenResult.ExpiresAtUtc.ToUnixTimeSeconds().ToString(),
            validationResult.ClaimsIdentity.FindFirst("exp")?.Value);

        Assert.False(string.IsNullOrWhiteSpace(validationResult.ClaimsIdentity.FindFirst("jti")?.Value));
    }
}