using System.Text.Json.Serialization;

namespace RawConsumer.Models;

public class IncomingReading
{
    [JsonPropertyName("event_id")]
    public string? EventId { get; set; }

    [JsonPropertyName("source_id")]
    public string? SourceId { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
