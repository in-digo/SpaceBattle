var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Endpoint позволяет внешним системам проверить доступность сервиса
app.MapGet("/health", () => Results.Ok());

app.Run();

public partial class Program
{
}