

namespace PersistenceConsumer.Configuration;

public class KafkaOptions
{
    public string BootstrapServers { get; set; } = null!;
    public string Topic { get; set; } = null!;
    public string GroupId { get; set; } = null!;
    public string ClientId { get; set; } = null!;
}
