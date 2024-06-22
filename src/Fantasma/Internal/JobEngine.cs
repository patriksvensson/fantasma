namespace Fantasma.Internal;

internal sealed class JobEngine : BackgroundService, IJobEngine
{
    private readonly IJobStorage _storage;
    private readonly IServiceProvider _services;

    public JobEngine(
        IJobStorage storage,
        IJobScheduler scheduler,
        IEnumerable<RecurringJob> recurringJobs,
        IServiceProvider services)
    {
        _storage = storage;
        _services = services ?? throw new ArgumentNullException(nameof(services));

        // Add all recurring jobs
        foreach (var recurringJob in recurringJobs)
        {
            scheduler.Schedule(
                recurringJob.Data,
                Trigger.Recurring(
                    recurringJob.Id,
                    recurringJob.Cron));
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            while (true)
            {
                var acquired = await _storage.Acquire();
                if (acquired == null)
                {
                    break;
                }

                await HandleJob(stoppingToken, acquired);
            }

            stoppingToken.WaitHandle.WaitOne(
                TimeSpan.FromSeconds(1));
        }
    }

    private async Task HandleJob(
        CancellationToken cancellationToken,
        Job acquired)
    {
        try
        {
            using var scope = _services.CreateScope();

            // Resolve the handler
            var handlerBase = typeof(IJobHandler<>);
            var handleType = handlerBase.MakeGenericType(acquired.Data.GetType());
            var handler = scope.ServiceProvider.GetService(handleType) as IJobHandler;
            if (handler == null)
            {
                throw new InvalidOperationException("Could not resolve job handler");
            }

            acquired.Status = JobStatus.Running;
            await handler.Handle(acquired.Data, cancellationToken);
            acquired.Status = JobStatus.Successful;
            await _storage.Release(acquired);
        }
        catch
        {
            acquired.Status = JobStatus.Failed;
            await _storage.Release(acquired);
        }
    }
}