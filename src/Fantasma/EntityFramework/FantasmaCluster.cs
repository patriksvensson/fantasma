namespace Fantasma.EntityFramework;

[PublicAPI]
public sealed class FantasmaCluster
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public required long Token { get; set; }
    public required long Elected { get; set; }
    public required DateTime LastHeartbeat { get; set; }

    internal bool IsAlive(TimeProvider time)
    {
        var heartbeat = DateTime.SpecifyKind(LastHeartbeat, DateTimeKind.Utc);
        var diff = time.GetUtcNow() - heartbeat;
        return diff.TotalMinutes < 1;
    }
}