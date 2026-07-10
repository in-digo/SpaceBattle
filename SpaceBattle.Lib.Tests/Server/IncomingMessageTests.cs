using System.Text.Json;

namespace SpaceBattle.Lib.Tests;

public class IncomingMessageTests
{
    //Сообщение от агента успешно десериализуется из JSON
    [Fact]
    public void Should_Deserialize_Valid_Json_To_IncomingMessage()
    {
        var json = @"{
            ""gameId"": ""550e8400-e29b-41d4-a716-446655440000"",
            ""objectId"": ""548"",
            ""operationId"": ""move.forward"",
            ""args"": { ""velocity"": 2 }
        }";

        var message = JsonSerializer.Deserialize<IncomingMessage>(json);

        Assert.NotNull(message);
        Assert.Equal("550e8400-e29b-41d4-a716-446655440000", message.GameId);
        Assert.Equal("548", message.ObjectId);
        Assert.Equal("move.forward", message.OperationId);
        Assert.NotNull(message.Args);
        Assert.True(message.Args.ContainsKey("velocity"));
    }
}