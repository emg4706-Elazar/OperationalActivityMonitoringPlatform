using System.Text.Json.Serialization;


namespace PersistenceConsumer.Models;

public class AnomalyMessage
{
    [JsonPropertyName("event_id")]
    public string EventId { get; set; } = null!;

    [JsonPropertyName("source_id")]
    public string SourceId { get; set; } = null!;

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = null!;

    [JsonPropertyName("value")]
    public double Value { get; set; }

    [JsonPropertyName("mean")]
    public double Mean { get; set; }

    [JsonPropertyName("standard_deviation")]
    public double StandardDeviation { get; set; }

    [JsonPropertyName("z_score")]
    public double? ZScore { get; set; }

    [JsonPropertyName("severity")]
    public string Severity { get; set; } = null!;

    [JsonPropertyName("detected_at")]
    public DateTimeOffset DetectedAt { get; set; }
}
