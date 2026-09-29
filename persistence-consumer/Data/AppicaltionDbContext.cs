using Microsoft.EntityFrameworkCore;
using PersistenceConsumer.Data.Entities;


namespace PersistenceConsumer.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }


    public DbSet<AnomalyEntity> Anomalies =>
        Set<AnomalyEntity>();
}
