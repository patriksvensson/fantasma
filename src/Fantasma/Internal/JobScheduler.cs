namespace Fantasma.Internal;

internal sealed class JobScheduler : IJobScheduler
{
    private readonly IJobProvider _provider;
    private readonly TimeProvider _time;

    public JobScheduler(IJobProvider provider, TimeProvider time)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _time = time ?? throw new ArgumentNullException(nameof(time));
    }

    public async Task<bool> Schedule(IJobData data, Trigger trigger)
    {
        var scheduledAt = GetNextExecutionTime(trigger, _time);
        if (scheduledAt == null)
        {
            return false;
        }

        var job = new Job
        {
            Id = trigger.Id,
            ScheduledAt = scheduledAt.Value,
            Status = JobStatus.Scheduled,
            Data = data,
            Cron = trigger.IsRecurring ? trigger.Cron : null,
        };

        using (var storage = _provider.GetStorage())
        {
            await storage.Add(job);
        }

        return true;
    }

    private static DateTimeOffset? GetNextExecutionTime(Trigger trigger, TimeProvider time)
    {
        if (trigger.IsDelayed)
        {
            return trigger.Time;
        }

        if (trigger.IsRecurring)
        {
            var expr = CronExpression.Parse(trigger.Cron, CronFormat.IncludeSeconds);
            var now = time.GetUtcNow().UtcDateTime;
            return expr.GetNextOccurrence(now);
        }

        return DateTimeOffset.Now;
    }
}