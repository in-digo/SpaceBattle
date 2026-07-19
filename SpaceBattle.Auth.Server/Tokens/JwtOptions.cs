namespace SpaceBattle.Auth.Server.Tokens;

public class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string SigningKey { get; set; } = string.Empty;

    public TimeSpan Lifetime { get; set; }
}