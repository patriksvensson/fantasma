namespace Fantasma.EntityFramework;

[PublicAPI]
public sealed class FantasmaHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public required string OriginalId { get; set; }
    public required string Name { get; set; }
    public required DateTimeOffset ScheduledAt { get; set; }
    public required DateTimeOffset CompletedAt { get; set; }
    public required JobStatus Status { get; set; }
    public required JobKind Kind { get; set; }
    public required string? Cron { get; set; }
    public required string? Error { get; set; }

    internal static FantasmaHistory FromCompleted(CompletedJob job)
    {
        return new FantasmaHistory
        {
            OriginalId = job.Id,
            Name = job.Name,
            ScheduledAt = job.ScheduledAt,
            CompletedAt = job.CompletedAt,
            Status = job.Status,
            Kind = job.Kind,
            Cron = job.Cron,
            Error = job.Error,
        };
    }
}