using PersistenceConsumer.Models;

namespace PersistenceConsumer.Repositories;

public interface IElasticsearchRepository
{
    Task UpsertAsync(
        AnomalyDocument document,
        CancellationToken cancellationToken);
}
