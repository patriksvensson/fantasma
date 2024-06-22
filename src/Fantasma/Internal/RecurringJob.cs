namespace Fantasma.Internal;

internal sealed class RecurringJob
{
    public string Id { get; }
    public string Cron { get; }
    public IJobData Data { get; }

    public RecurringJob(string id, string cron, IJobData data)
    {
        Id = id;
        Cron = cron;
        Data = data;
    }
}