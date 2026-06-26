using System.Text.Json.Serialization;

namespace SpaceBattle.Lib;

public class IncomingMessage
{
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = string.Empty;
    
    [JsonPropertyName("objectId")]
    public string ObjectId { get; set; } = string.Empty;
    
    [JsonPropertyName("operationId")]
    public string OperationId { get; set; } = string.Empty;

    [JsonPropertyName("args")]
    public IDictionary<string, object> Args { get; set; } = new Dictionary<string, object>();
}