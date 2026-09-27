using RawConsumer.Models;
using System.Globalization;

namespace RawConsumer.Services;

public static class ReadingValidationService
{
    public static bool Validate(
        IncomingReading reading,
        out RawReading? validReading,
        out string? error)
    {
        error = null;
        validReading = null;

        if (string.IsNullOrWhiteSpace(reading.EventId))
        {
            error = "event_id is missing";
            return false;
        }

        if (string.IsNullOrWhiteSpace(reading.SourceId))
        {
            error = "source_id is missing";
            return false;
        }

        if (!DateTimeOffset.TryParse(
            reading.Timestamp,
            out DateTimeOffset timestamp))
        {
            error = $"Invalid timestamp: {reading.Timestamp}.";
            return false;
        }

        if (!double.TryParse(
            reading.Value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out double value))
        {
            error = $"Invalid value: {reading.Value}.";
            return false;
        }

        validReading = new RawReading
        {
            EventId = reading.EventId,
            SourceId = reading.SourceId,
            Timestamp = timestamp.UtcDateTime,
            Value = value
        };

        return true;
    }
}
