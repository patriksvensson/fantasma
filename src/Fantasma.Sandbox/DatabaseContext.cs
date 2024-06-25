using Fantasma.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Fantasma.Sandbox;

public class DatabaseContext : DbContext, IFantasmaDatabase
{
    public DbSet<FantasmaJob> Jobs => Set<FantasmaJob>();
    public DbSet<FantasmaCluster> Cluster => Set<FantasmaCluster>();

    public DatabaseContext(
        DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }
}