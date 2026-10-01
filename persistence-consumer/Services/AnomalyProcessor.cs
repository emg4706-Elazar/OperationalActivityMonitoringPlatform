using PersistenceConsumer.Mappings;
using PersistenceConsumer.Data.Entities;
using PersistenceConsumer.Repositories;
using PersistenceConsumer.Models;


namespace PersistenceConsumer.Services;

public class AnomalyProcessor
{
    private readonly IMySqlRepository _mySqlRepository;
    private readonly IElasticsearchRepository _elasticRepository;
    private readonly IRabbitMqPublisher _rabbitMqPublisher;
    
    public AnomalyProcessor(
        IMySqlRepository mySqlRepository,
        IElasticsearchRepository elasticRepository,
        IRabbitMqPublisher rabbitMqPublisher)
    {
        _mySqlRepository = mySqlRepository;
        _elasticRepository = elasticRepository;
        _rabbitMqPublisher = rabbitMqPublisher;
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


        if (string.Equals(
            entity.Severity,
            "Critical",
            StringComparison.OrdinalIgnoreCase))
        {
            CriticalAlertMessage alert = new()
            {
                AnomalyId = entity.Id,
                SourceId = entity.SourceId,
                Severity = entity.Severity,
                DetectedAt = entity.DetectedAt
            };

            await _rabbitMqPublisher.PublishAsync(
                alert,
                cancellationToken);
        }
    }
}
