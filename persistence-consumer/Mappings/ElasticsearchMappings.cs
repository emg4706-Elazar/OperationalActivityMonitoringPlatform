using PersistenceConsumer.Data.Entities;
using PersistenceConsumer.Models;

namespace PersistenceConsumer.Mappings;

public static class ElasticsearchMappings
{
    public static AnomalyDocument ToElasticsearch(
        this AnomalyEntity anomaly,
        string sector)
    {
        return new AnomalyDocument
        {
            Id = anomaly.Id,
            EventId = anomaly.EventId,
            SourceId = anomaly.SourceId,
            Value = anomaly.Value,
            Mean = anomaly.Mean,
            Sector = sector,
            StandardDeviation =
                anomaly.StandardDeviation,
            ZScore = anomaly.ZScore
                ?? throw new InvalidOperationException(
                    "Anomaly ZScore cannot be null"),
            Severity = anomaly.Severity,
            Status = anomaly.Status,
            DetectedAt = anomaly.DetectedAt
        };
    }
}
