namespace Fantasma.EntityFramework;

[PublicAPI]
public sealed class FantasmaJob
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id { get; set; } = null!;

    public required string Name { get; set; }
    public required DateTimeOffset ScheduledAt { get; set; }
    public required JobStatus Status { get; set; }
    public required JobKind Kind { get; set; }
    public required string Data { get; set; }
    public required string ClrType { get; set; }
    public required string? Cron { get; set; }
}