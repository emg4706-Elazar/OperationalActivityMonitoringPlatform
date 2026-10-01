

namespace PersistenceConsumer.Models;

public sealed class CriticalAlertMessage
{
    public long AnomalyId { get; set; }
    public required string SourceId { get; set; }
    public required string Severity { get; set; }
    public DateTimeOffset DetectedAt { get; set; }
}
