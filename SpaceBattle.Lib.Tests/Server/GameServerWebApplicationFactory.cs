using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SpaceBattle.Lib;

namespace SpaceBattle.Lib.Tests;

public class GameServerWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string ISSUER = "SpaceBattle.Auth.Server";
    public const string AUDIENCE = "SpaceBattle.Game.Server";

    public const string SIGNING_KEY =  "integration-test-signing-key-with-at-least-32-characters";

    public TestMessageEndpoint MessageEndpoint { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            var testSettings = new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = ISSUER,
                ["Jwt:Audience"] = AUDIENCE,
                ["Jwt:SigningKey"] = SIGNING_KEY
            };

            // Тестовая конфигурация
            configurationBuilder.AddInMemoryCollection(testSettings);
        });

        builder.ConfigureServices(services =>
        {
            // Тесты авторизации не зависят от настройки статического игрового IoC
            services.RemoveAll<IMessageEndpoint>();
            services.AddSingleton<IMessageEndpoint>(MessageEndpoint);
        });
    }

    public class TestMessageEndpoint : IMessageEndpoint
    {
        public string? HandledJson { get; private set; }

        public void Handle(string json)
        {
            HandledJson = json;
        }

        public void Reset()
        {
            HandledJson = null;
        }
    }
}