namespace Fantasma;

public sealed class CompletedJob
{
    private Job _job;

    public string Id => _job.Id;
    public string Name => _job.Name;
    public DateTimeOffset ScheduledAt => _job.ScheduledAt;
    public DateTimeOffset CompletedAt { get; }
    public JobStatus Status => _job.Status;
    public JobKind Kind => _job.Kind;
    public string? Cron => _job.Cron;
    public string? Error { get; }

    public CompletedJob(Job job, DateTimeOffset completedAt, string? error)
    {
        _job = job ?? throw new ArgumentNullException(nameof(job));
        CompletedAt = completedAt;
        Error = error;
    }

    public Job? Reschedule(TimeProvider time)
    {
        return _job.Reschedule(time);
    }
}