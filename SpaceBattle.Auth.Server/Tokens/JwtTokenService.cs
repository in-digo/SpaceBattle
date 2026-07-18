using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace SpaceBattle.Auth.Server.Tokens;

public class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly TimeProvider _timeProvider;

    public JwtTokenService(
        JwtOptions options,
        TimeProvider timeProvider)
    {
        _options = options;
        _timeProvider = timeProvider;
    }

    public TokenResult CreateToken(Guid gameId, string userId)
    {
        var currentTime = _timeProvider.GetUtcNow();
        var expiresAtUtc = currentTime.Add(_options.Lifetime);

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SigningKey));

        var signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = currentTime.UtcDateTime,
            Expires = expiresAtUtc.UtcDateTime,
            Claims = new Dictionary<string, object>
            {
                ["sub"] = userId,
                ["game_id"] = gameId.ToString(),
                ["jti"] = Guid.NewGuid().ToString()
            },
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JsonWebTokenHandler();
        var accessToken = tokenHandler.CreateToken(tokenDescriptor);

        return new TokenResult(accessToken, expiresAtUtc);
    }
}