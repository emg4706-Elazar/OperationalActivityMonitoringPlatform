using PersistenceConsumer.Mappings;
using PersistenceConsumer.Data.Entities;
using PersistenceConsumer.Repositories;
using PersistenceConsumer.Models;
using Microsoft.Extensions.Primitives;


namespace PersistenceConsumer.Services;

public class AnomalyProcessor
{
    private readonly IMySqlRepository _mySqlRepository;
    private readonly IElasticsearchRepository _elasticRepository;
    
    public AnomalyProcessor(
        IMySqlRepository mySqlRepository,
        IElasticsearchRepository elasticRepository)
    {
        _mySqlRepository = mySqlRepository;
        _elasticRepository = elasticRepository;
    }

    public async Task ProcessAsync(
        AnomalyMessage message,
        CancellationToken cancellationToken)
    {
        AnomalyEntity entity =
            await _mySqlRepository
            .SaveOrGetExistingAsync(
                message.ToMySqlEntity(),
                cancellationToken);

        string? sector =
            await _mySqlRepository
            .GetSectorBySourceIdAsync(entity.SourceId,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(sector))
        {
            throw new InvalidOperationException(
                $"Station '{entity.SourceId}' was not found");
        }


        AnomalyDocument document =
            entity.ToElasticsearch(sector);

        await _elasticRepository.UpsertAsync(
            document,
            cancellationToken);


        
    }
}
