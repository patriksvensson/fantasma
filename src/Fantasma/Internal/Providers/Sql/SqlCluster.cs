using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fantasma.Internal;

internal sealed class SqlCluster : ICluster
{
    private readonly SqlHeartbeatThread _heartbeat;
    private readonly TimeProvider _time;
    private readonly ILogger<SqlCluster> _logger;
    private readonly IServiceScope _scope;
    private readonly IFantasmaDatabase _database;

    public bool IsLeader { get; private set; }
    public bool HasPeers { get; private set; }

    public SqlCluster(
        SqlHeartbeatThread heartbeat,
        TimeProvider time,
        ILogger<SqlCluster> logger,
        IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _heartbeat = heartbeat ?? throw new ArgumentNullException(nameof(heartbeat));
        _time = time ?? throw new ArgumentNullException(nameof(time));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scope = services.CreateScope();
        _database = _scope.ServiceProvider.GetRequiredService<IFantasmaDatabase>();
    }

    public void Dispose()
    {
        _database.Dispose();
        _scope.Dispose();
    }

    public Task Connect(CancellationToken cancellationToken)
    {
        // Start the heartbeat thread
        _logger.LogInformation("Starting heartbeat thread...");
        _heartbeat.Start(cancellationToken);

        // Wait for the heartbeat thread to announce us
        _logger.LogInformation("Waiting for cluster announcement...");
        var handles = new[] { _heartbeat.Announced, cancellationToken.WaitHandle, };
        var result = WaitHandle.WaitAny(handles, TimeSpan.FromSeconds(30));

        // Timeout?
        if (result == WaitHandle.WaitTimeout)
        {
            // TODO: Add retry
            throw new InvalidOperationException("Heartbeat thread never announced");
        }

        return Task.CompletedTask;
    }

    public async Task Update(CancellationToken cancellationToken)
    {
        // Is there consensus already?
        var self = await GetClusterNode(cancellationToken);
        if (self.Elected != 0 && await HasConsensus(cancellationToken))
        {
            return;
        }

        // Get all nodes we consider alive
        var nodes = (await _database.FantasmaCluster.AsNoTracking().ToListAsync(cancellationToken))
            .Where(x => x.IsAlive(_time))
            .ToList();

        // Find which one we think is the leader
        var leaderToken = nodes.MaxBy(x => x.Token)?.Token;
        if (leaderToken == null)
        {
            throw new InvalidOperationException("No cluster node had the highest token");
        }

        // Update our preferences
        self.Elected = leaderToken.Value;
        await _database.SaveChangesAsync(cancellationToken);

        // Only ourselves? Then we have automatic consensus
        if (nodes.Count == 1 && nodes[0].Token == _heartbeat.Token)
        {
            if (!IsLeader)
            {
                _logger.LogInformation("No other peers makes us the cluster leader");
            }

            IsLeader = true;
            HasPeers = false;
            return;
        }

        // Reach consensus
        while (!await HasConsensus(cancellationToken))
        {
            // Wait for a little while
            _logger.LogInformation("All cluster nodes have not yet reached consensus. Waiting...");
            cancellationToken.WaitHandle.WaitOne(
                TimeSpan.FromSeconds(10));
        }

        var wasLeader = IsLeader;
        IsLeader = _heartbeat.Token == leaderToken.Value;
        HasPeers = nodes.Count > 1;

        if (wasLeader != IsLeader)
        {
            _logger.LogInformation(
                IsLeader
                    ? "We are now the cluster leader"
                    : "We are no longer the cluster leader");
        }
    }

    public async Task Disconnect(CancellationToken cancellationToken)
    {
        // Stop the heartbeat
        _logger.LogInformation("Stopping heartbeat thread...");
        _heartbeat.Stop();

        // Remove the cluster node
        _logger.LogInformation("Disconnecting from cluster...");
        var node = await GetClusterNode(cancellationToken);
        _database.Remove(node);
        await _database.SaveChangesAsync(cancellationToken);
    }

    private async Task<FantasmaCluster> GetClusterNode(CancellationToken cancellationToken)
    {
        var node = await _database.FantasmaCluster.SingleOrDefaultAsync(x => x.Token == _heartbeat.Token, cancellationToken);
        if (node == null)
        {
            throw new InvalidOperationException("Could not get cluster node");
        }

        return node;
    }

    private async Task<bool> HasConsensus(CancellationToken cancellationToken)
    {
        return (await _database.FantasmaCluster.AsNoTracking().ToListAsync(cancellationToken))
            .Where(x => x.IsAlive(_time))
            .ToList()
            .DistinctBy(x => x.Elected)
            .Count() == 1;
    }
}