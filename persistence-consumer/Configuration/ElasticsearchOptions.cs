

namespace PersistenceConsumer.Configuration;

public class ElasticsearchOptions
{
    public string Url { get; set; } = null!;
    public string IndexName { get; set; } = null!;
}
