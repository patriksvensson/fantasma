namespace Fantasma.Internal;

internal sealed class InMemoryStorage : IJobStorage
{
    private readonly TimeProvider _time;
    private readonly List<Job> _jobs;

    public InMemoryStorage(TimeProvider time)
    {
        _time = time ?? throw new ArgumentNullException(nameof(time));
        _jobs = new List<Job>();
    }

    public Task Add(Job job)
    {
        // Already added?
        if (_jobs.Any(x => x.Id == job.Id))
        {
            return Task.CompletedTask;
        }

        _jobs.Add(job);
        return Task.CompletedTask;
    }

    public Task Remove(Job job)
    {
        _jobs.Remove(job);

        return Task.CompletedTask;
    }

    public Task<Job?> Acquire()
    {
        var now = _time.GetUtcNow().UtcDateTime;
        var job = _jobs.Where(x => x.ScheduledAt < now).MinBy(x => x.ScheduledAt);
        if (job == null)
        {
            return Task.FromResult<Job?>(null);
        }

        _jobs.Remove(job);

        return Task.FromResult<Job?>(job);
    }

    public Task Release(Job job)
    {
        if (job.Cron != null)
        {
            var rescheduled = job.Reschedule(_time);
            if (rescheduled != null)
            {
                _jobs.Add(rescheduled);
            }
        }

        return Task.CompletedTask;
    }
}