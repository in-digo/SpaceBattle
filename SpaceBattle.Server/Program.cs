using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using SpaceBattle.Server.Authorization;
using SpaceBattle.Lib;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var issuer = builder.Configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "Не настроен Jwt:Issuer.");

        var audience = builder.Configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "Не настроен Jwt:Audience.");

        var signingKey = builder.Configuration["Jwt:SigningKey"];

        if (string.IsNullOrWhiteSpace(signingKey))
        {
            throw new InvalidOperationException(
                "Не настроен Jwt:SigningKey.");
        }

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(signingKey)),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ClockSkew = TimeSpan.Zero,
                ValidAlgorithms = new[]
                {
                    SecurityAlgorithms.HmacSha256
                }
            };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        GameAuthorizationConstants.POLICY_NAME,
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.AddRequirements(new GameAccessRequirement());
        });
});

builder.Services.AddSingleton<IAuthorizationHandler, GameAccessAuthorizationHandler>();
builder.Services.AddTransient< IMessageEndpoint, MessageEndpoint>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/game/message", async (
    HttpContext context, 
    IAuthorizationService authorizationService, 
    IMessageEndpoint messageEndpoint) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var json = await reader.ReadToEndAsync();

    IncomingMessage? message;

    try
    {
        message = JsonSerializer.Deserialize<IncomingMessage>(json);
    }
    catch (JsonException)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    if (message is null ||
        string.IsNullOrWhiteSpace(message.GameId))
    {
        context.Response.StatusCode =  StatusCodes.Status400BadRequest;
        return;
    }

    var authorizationResult = await authorizationService.AuthorizeAsync(
        context.User,
        message.GameId,
        GameAuthorizationConstants.POLICY_NAME);

    if (!authorizationResult.Succeeded)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }

    try
    {
        messageEndpoint.Handle(json);
        context.Response.StatusCode = StatusCodes.Status200OK;
    }
    catch (Exception)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
})
.RequireAuthorization();

app.Run();

public partial class Program
{
}