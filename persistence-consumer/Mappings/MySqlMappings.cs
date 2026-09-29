using PersistenceConsumer.Models;
using PersistenceConsumer.Data.Entities;


namespace PersistenceConsumer.Mappings;

public static class MySqlMappings
{
    public static AnomalyEntity ToMySqlEntity(
        AnomalyMessage message)
    {
        return new AnomalyEntity
        {
            EventId = message.EventId,
            SourceId = message.SourceId,
            Value = message.Value,
            Mean = message.Mean,
            StandardDeviation = message.StandardDeviation,
            ZScore = message.ZScore,
            Severity = message.Severity,
            DetectedAt = message.DetectedAt.UtcDateTime
        };
    }
}
