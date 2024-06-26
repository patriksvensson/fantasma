namespace Fantasma.Internal;

internal sealed class JobEngine : BackgroundService, IJobEngine
{
    private readonly ICluster _cluster;
    private readonly IJobProvider _provider;
    private readonly IServiceProvider _services;
    private readonly TimeProvider _time;
    private readonly SleepPreference? _sleepPreference;

    public JobEngine(
        IJobScheduler scheduler,
        ICluster cluster,
        IJobProvider provider,
        IEnumerable<RecurringJob> recurringJobs,
        IServiceProvider services,
        TimeProvider time,
        SleepPreference? sleepPreference = null)
    {
        _cluster = cluster ?? throw new ArgumentNullException(nameof(cluster));
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _time = time ?? throw new ArgumentNullException(nameof(time));
        _sleepPreference = sleepPreference;

        // Add all recurring jobs
        foreach (var recurringJob in recurringJobs)
        {
            scheduler.Schedule(
                recurringJob.Name,
                recurringJob.Data,
                Trigger.Recurring(
                    recurringJob.Id,
                    recurringJob.Cron));
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sleep = _sleepPreference?.Time ?? _provider.Sleep;
        if (sleep.TotalSeconds < 1)
        {
            sleep = TimeSpan.FromSeconds(1);
        }

        await Task.Yield();

        // Connect to the cluster
        await _cluster.Connect(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            // Update the cluster
            await _cluster.Update(stoppingToken);

            // Are we the leader?
            if (_cluster.IsLeader)
            {
                using (var storage = _provider.GetStorage())
                {
                    while (_cluster.IsLeader)
                    {
                        var data = await storage.GetNextJob();
                        if (data == null)
                        {
                            break;
                        }

                        await ProcessJob(storage, data, stoppingToken);

                        // Update the cluster
                        await _cluster.Update(stoppingToken);
                    }
                }
            }

            // Wait for a while until continuing.
            stoppingToken.WaitHandle.WaitOne(sleep);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _cluster.Disconnect(cancellationToken);
    }

    private async Task ProcessJob(
        IJobStorage storage,
        Job job,
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _services.CreateScope();

            // Resolve the handler
            var handleType = typeof(IJobHandler<>).MakeGenericType(job.Data.GetType());
            if (scope.ServiceProvider.GetService(handleType) is not IJobHandler handler)
            {
                // TODO: Validate this at schedule time?
                throw new InvalidOperationException("Could not resolve job handler");
            }

            await storage.UpdateJob(job, j => j.Status = JobStatus.Running);
            await handler.Handle(job.Data, cancellationToken);

            // Release the job
            var completed = GetCompletedJob(job, JobStatus.Successful, exception: null);
            await storage.Release(completed);
        }
        catch (Exception ex)
        {
            var completed = GetCompletedJob(job, JobStatus.Failed, ex);
            await storage.Release(completed);
        }
    }

    private CompletedJob GetCompletedJob(Job job, JobStatus status, Exception? exception)
    {
        job.Status = status;
        return new CompletedJob(job, _time.GetUtcNow(), exception?.Message);
    }
}