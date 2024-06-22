namespace Fantasma;

[PublicAPI]
public sealed class Job
{
    public required string Id { get; set; }
    public required DateTimeOffset ScheduledAt { get; set; }
    public required JobStatus Status { get; set; }
    public required IJobData Data { get; set; }
    public string? Cron { get; set; }

    public Job? Reschedule(TimeProvider time)
    {
        if (Cron == null)
        {
            return null;
        }

        var cron = CronExpression.Parse(Cron, CronFormat.IncludeSeconds);
        var now = time.GetUtcNow().UtcDateTime;
        var scheduledAt = cron.GetNextOccurrence(now);
        if (scheduledAt == null)
        {
            return null;
        }

        return new Job
        {
            Id = Id,
            Cron = Cron,
            Data = Data,
            ScheduledAt = new DateTimeOffset(scheduledAt.Value),
            Status = JobStatus.Scheduled,
        };
    }
}