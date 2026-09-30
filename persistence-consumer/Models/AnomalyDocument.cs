using System.Text.Json.Serialization;

namespace PersistenceConsumer.Models;

public class AnomalyDocument
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("event_id")]
    public string EventId { get; set; } = null!;

    [JsonPropertyName("source_id")]
    public string SourceId { get; set; } = null!;

    [JsonPropertyName("sector")]
    public string Sector { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public double Value { get; set; }

    [JsonPropertyName("mean")]
    public double Mean { get; set; }

    [JsonPropertyName("standard_deviation")]
    public double StandardDeviation { get; set; }

    [JsonPropertyName("z_score")]
    public double ZScore { get; set; }

    [JsonPropertyName("severity")]
    public string Severity { get; set; } = null!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = null!;

    [JsonPropertyName("detected_at")]
    public DateTimeOffset DetectedAt { get; set; }
}
