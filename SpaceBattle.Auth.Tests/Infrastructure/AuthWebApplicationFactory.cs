using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace SpaceBattle.Auth.Tests.Infrastructure;

public class AuthWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string ISSUER = "SpaceBattle.Auth.Server";
    public const string AUDIENCE = "SpaceBattle.Game.Server";

    public const string SIGNING_KEY = "integration-test-signing-key-with-at-least-32-characters";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            var testSettings = new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = ISSUER,
                ["Jwt:Audience"] = AUDIENCE,
                ["Jwt:SigningKey"] = SIGNING_KEY,
                ["Jwt:Lifetime"] = "00:15:00"
            };

            // Тестовая конфигурация
            configurationBuilder.AddInMemoryCollection(testSettings);
        });
    }
}