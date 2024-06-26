using Fantasma.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Fantasma.Sandbox;

public class DatabaseContext : DbContext, IFantasmaDatabase
{
    public DbSet<FantasmaJob> FantasmaJobs => Set<FantasmaJob>();
    public DbSet<FantasmaCluster> FantasmaCluster => Set<FantasmaCluster>();
    public DbSet<FantasmaHistory> FantasmaHistory => Set<FantasmaHistory>();

    public DatabaseContext(
        DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FantasmaCluster>().ToTable("Cluster");
        modelBuilder.Entity<FantasmaJob>().ToTable("Jobs");
        modelBuilder.Entity<FantasmaHistory>().ToTable("History");
    }
}