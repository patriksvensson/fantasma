namespace Fantasma.Internal;

internal sealed class JobScheduler : IJobScheduler
{
    private readonly IJobStorage _storage;
    private readonly TimeProvider _time;

    public JobScheduler(IJobStorage storage, TimeProvider time)
    {
        _storage = storage;
        _time = time ?? throw new ArgumentNullException(nameof(time));
    }

    public async Task<bool> Schedule(IJobData data, Trigger trigger)
    {
        var scheduledAt = trigger.GetNextExecutionTime(_time);
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
            Cron = trigger.IsRecurring ? trigger.Cron.ToString() : null,
        };

        await _storage.Add(job);
        return true;
    }
}