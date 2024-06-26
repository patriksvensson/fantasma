namespace Fantasma;

[PublicAPI]
public sealed class Trigger
{
    [MemberNotNullWhen(true, nameof(Time))]
    internal bool IsDelayed => Time != null;

    [MemberNotNullWhen(true, nameof(Cron))]
    internal bool IsRecurring => Id != null && Cron != null;

    internal string Id { get; }
    internal DateTimeOffset? Time { get; }
    internal string? Cron { get; }

    public static Trigger Now
    {
        get
        {
            return new Trigger(Guid.NewGuid().ToString());
        }
    }

    private Trigger(string id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
    }

    private Trigger(string id, DateTimeOffset time)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Time = time;
    }

    private Trigger(string id, string cron)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Cron = cron ?? throw new ArgumentNullException(nameof(cron));
    }

    public static Trigger AtTime(DateTimeOffset time)
    {
        return new Trigger(Guid.NewGuid().ToString(), time);
    }

    public static Trigger Recurring(string id, string cron)
    {
        return new Trigger(id, cron);
    }

    public JobKind GetJobKind()
    {
        if (Time != null)
        {
            return JobKind.Delayed;
        }

        if (Cron != null)
        {
            return JobKind.Recurring;
        }

        return JobKind.Queued;
    }
}