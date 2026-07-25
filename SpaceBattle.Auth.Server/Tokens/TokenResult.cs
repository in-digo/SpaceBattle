namespace SpaceBattle.Auth.Server.Tokens;

public class TokenResult
{
    public TokenResult(string accessToken, DateTimeOffset expiresAtUtc)
    {
        AccessToken = accessToken;
        ExpiresAtUtc = expiresAtUtc;
    }

    public string AccessToken { get; }

    public DateTimeOffset ExpiresAtUtc { get; }
}