using SpaceBattle.Lib;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/game/message", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var json = await reader.ReadToEndAsync();

    try
    {
        var endpoint = new MessageEndpoint();
        endpoint.Handle(json);
        context.Response.StatusCode = 200;
    }
    catch (Exception)
    {
        context.Response.StatusCode = 400;
    }
});

app.Run();