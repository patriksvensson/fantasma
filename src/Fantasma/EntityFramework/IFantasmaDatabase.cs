using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Fantasma.EntityFramework;

[PublicAPI]
public interface IFantasmaDatabase : IDisposable
{
    DbSet<FantasmaJob> FantasmaJobs { get; }
    DbSet<FantasmaCluster> FantasmaCluster { get; }
    DbSet<FantasmaHistory> FantasmaHistory { get; }

    EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
        where TEntity : class;

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}