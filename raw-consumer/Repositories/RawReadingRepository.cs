using RawConsumer.Models;
using MongoDB.Driver;

namespace RawConsumer.Repositories;

public class RawReadingRepository :
    IRawReadingRepository
{
    private readonly IMongoCollection<RawReading> _collection;

    public RawReadingRepository(
        IMongoCollection<RawReading> collection)
    {
        _collection = collection;
    }

    public async Task CreateIndexesAsync(
        CancellationToken cancellationToken)
    {
        var indexKeys = Builders<RawReading>
            .IndexKeys
            .Ascending(reading => reading.EventId);

        var indexOptions = new CreateIndexOptions
        {
            Unique = true,
            Name = "ux_raw_readings_event_id"
        };

        var indexModel =
            new CreateIndexModel<RawReading>(
                indexKeys,
                indexOptions);

        await _collection.Indexes.CreateOneAsync(
            indexModel,
            cancellationToken: cancellationToken);
    }


    public async Task UpsertAsync(
        RawReading reading,
        CancellationToken cancellationToken)
    {
        var filter = Builders<RawReading>
            .Filter
            .Eq(item => item.EventId, reading.EventId);

        var update = Builders<RawReading>
            .Update
            .SetOnInsert(
                item => item.EventId,
                reading.EventId)
            .Set(
                item => item.SourceId,
                reading.SourceId)
            .Set(
                item => item.Timestamp,
                reading.Timestamp)
            .Set(
                item => item.Value,
                reading.Value);

        var options = new UpdateOptions
        {
            IsUpsert = true
        };

        await _collection.UpdateOneAsync(
            filter,
            update,
            options,
            cancellationToken);
    }
}
