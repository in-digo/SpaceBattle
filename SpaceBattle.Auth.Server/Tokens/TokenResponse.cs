namespace SpaceBattle.Auth.Server.Tokens;

public class TokenResponse
{
    public string AccessToken { get; }
    public DateTimeOffset ExpiresAtUtc { get; }

    public TokenResponse(
        string accessToken,
        DateTimeOffset expiresAtUtc)
    {
        AccessToken = accessToken;
        ExpiresAtUtc = expiresAtUtc;
    }
}