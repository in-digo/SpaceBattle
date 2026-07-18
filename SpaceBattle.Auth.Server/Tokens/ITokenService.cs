namespace SpaceBattle.Auth.Server.Tokens;

public interface ITokenService
{
    TokenResult CreateToken(Guid gameId, string userId);
}