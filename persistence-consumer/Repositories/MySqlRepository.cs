using Microsoft.EntityFrameworkCore;
using PersistenceConsumer.Data;
using PersistenceConsumer.Data.Entities;


namespace PersistenceConsumer.Repositories;

public class MySqlRepository :IMySqlRepository
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public MySqlRepository(
        IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }


    public async Task<AnomalyEntity> SaveOrGetExistingAsync(
    AnomalyEntity entity,
    CancellationToken cancellationToken)
    {
        await using ApplicationDbContext context =
            await _contextFactory.CreateDbContextAsync(
                cancellationToken);

        var existing = await context.Anomalies
            .FirstOrDefaultAsync(anomaly => 
            anomaly.EventId == entity.EventId,
            cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        context.Anomalies.Add(entity);

        await context.SaveChangesAsync(cancellationToken);

        return entity;
    }
}
