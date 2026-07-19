using Microsoft.AspNetCore.Authorization;

namespace SpaceBattle.Server.Authorization;

public class GameAccessAuthorizationHandler : AuthorizationHandler<GameAccessRequirement, string>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        GameAccessRequirement requirement,
        string resource)
    {
        var tokenGameId = context.User
            .FindFirst(GameAuthorizationConstants.GAME_ID_CLAIM_NAME)
            ?.Value;

        if (string.Equals(tokenGameId, resource, StringComparison.Ordinal))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}