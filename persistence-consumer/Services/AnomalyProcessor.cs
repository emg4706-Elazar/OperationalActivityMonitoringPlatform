using PersistenceConsumer.Mappings;
using PersistenceConsumer.Data.Entities;
using PersistenceConsumer.Repositories;
using PersistenceConsumer.Models;


namespace PersistenceConsumer.Services;

public class AnomalyProcessor
{
    private readonly IMySqlRepository _mySqlRepository;
    
    public AnomalyProcessor(
        IMySqlRepository mySqlRepository)
    {
        _mySqlRepository = mySqlRepository;
    }

    public async Task Process(
        AnomalyMessage message,
        CancellationToken cancellationToken)
    {
        AnomalyEntity entity =
            await _mySqlRepository
            .SaveOrGetExistingAsync(
                message.ToMySqlEntity(),
                cancellationToken);
    }
}
