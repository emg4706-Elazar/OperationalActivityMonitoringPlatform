using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RawConsumer.Models;

public class RawReading
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("event_id")]
    public string EventId { get; set; } = null!;

    [BsonElement("source_id")]
    public string SourceId { get; set; } = null!;

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }

    [BsonElement("value")]
    public Double Value { get; set; }
}
