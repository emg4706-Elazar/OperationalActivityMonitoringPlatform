


namespace PersistenceConsumer.Data.Entities;

public class AnomalyEntity
{
    public long Id { get; set; }
    public string EventId { get; set; } = null!;
    public string SourceId { get; set; } = null!;
    public double Value { get; set; }
    public double Mean { get; set; }
    public double StandardDeviation { get; set; }
    public double? ZScore { get; set; }
    public string Severity { get; set; } = null!;
    public string? Status { get; set; }
    public DateTime DetectedAt { get; set; }
}
