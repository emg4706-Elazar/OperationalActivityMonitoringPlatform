

namespace PersistenceConsumer.Data.Entities;

public class StationEntity
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Sector { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
