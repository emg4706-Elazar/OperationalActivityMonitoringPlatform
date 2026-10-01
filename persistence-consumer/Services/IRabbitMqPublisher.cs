using PersistenceConsumer.Models;

namespace PersistenceConsumer.Services;

public interface IRabbitMqPublisher
{
    Task PublishAsync(
        CriticalAlertMessage message,
        CancellationToken cancellationToken);
}
