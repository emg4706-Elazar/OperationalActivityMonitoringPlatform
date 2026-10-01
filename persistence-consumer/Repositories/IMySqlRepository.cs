using PersistenceConsumer.Data.Entities;

namespace PersistenceConsumer.Repositories;

public interface IMySqlRepository
{
    Task<AnomalyEntity> SaveOrGetExistingAsync(
    AnomalyEntity entity,
    CancellationToken cancellationToken);

    Task<string?> GetSectorBySourceIdAsync(
        string sourceId,
        CancellationToken cancellationToken);
}
