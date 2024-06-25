using Microsoft.Extensions.Logging;

namespace Fantasma.Internal;

internal sealed class SqlHeartbeatThread
{
    private readonly TimeProvider _time;
    private readonly IServiceProvider _services;
    private readonly ILogger<SqlHeartbeatThread> _logger;
    private readonly ManualResetEvent _stopped;
    private readonly ManualResetEvent _stopping;
    private readonly ManualResetEvent _announced;
    private readonly object _lock;
    private Thread? _thread;

    public long Token { get; }
    public WaitHandle Announced => _announced;

    public SqlHeartbeatThread(
        TimeProvider time,
        IServiceProvider services,
        ILogger<SqlHeartbeatThread> logger)
    {
        _time = time;
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stopped = new ManualResetEvent(true);
        _stopping = new ManualResetEvent(false);
        _announced = new ManualResetEvent(false);
        _lock = new object();

        Token = Random.Shared.NextInt64();
    }

    public void Start(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            if (!_stopped.WaitOne(0))
            {
                return;
            }

            _thread = new Thread(() => Execute(cancellationToken))
            {
                IsBackground = true,
            };

            _thread.Start();
            _stopped.Set();
        }
    }

    public void Stop()
    {
        lock (_lock)
        {
            if (_stopped.WaitOne(0))
            {
                return;
            }

            if (_thread != null)
            {
                _stopping.Set();
                _thread.Join();
                _thread = null;
            }
        }
    }

    private void Execute(CancellationToken cancellationToken)
    {
        var handles = new[]
        {
            _stopping, cancellationToken.WaitHandle,
        };

        while (true)
        {
            _logger.LogInformation("Sending heartbeat...");

            using (var scope = _services.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<IFantasmaDatabase>())
            {
                var node = GetClusterNode(context);
                node.LastHeartbeat = _time.GetUtcNow().DateTime;
                context.SaveChanges();

                if (!_announced.WaitOne(0))
                {
                    // We're now announced
                    _logger.LogInformation("Cluster node has been announced");
                    _announced.Set();
                }
            }

            // Wait for 30 seconds
            var result = WaitHandle.WaitAny(handles, TimeSpan.FromSeconds(30));
            if (result is 0 or 1)
            {
                break;
            }
        }

        _stopped.Reset();
        _stopping.Reset();
    }

    private FantasmaCluster GetClusterNode(IFantasmaDatabase context)
    {
        var node = context.Cluster.SingleOrDefault(x => x.Token == Token);
        if (node == null)
        {
            node = context.Cluster.Add(
                    new FantasmaCluster
                    {
                        Elected = 0,
                        Token = Token,
                    })
                .Entity;

            context.SaveChanges();
        }

        return node;
    }
}