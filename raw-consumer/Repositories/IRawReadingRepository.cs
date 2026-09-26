using RawConsumer.Models;

namespace RawConsumer.Repositories;

public interface IRawReadingRepository
{
    Task CreateIndexesAsync(
        CancellationToken cancellationToken);

    Task UpsertAsync(
        RawReading reading,
        CancellationToken cancellationToken);
}
