using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fantasma.EntityFramework;

[PublicAPI]
public sealed class FantasmaJob
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id { get; set; } = null!;

    public required DateTimeOffset ScheduledAt { get; set; }
    public required JobStatus Status { get; set; }
    public required string Data { get; set; }
    public required string ClrType { get; set; }
    public string? Cron { get; set; }
}