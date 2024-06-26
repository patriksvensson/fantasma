namespace Fantasma.Internal;

internal sealed class RecurringJob
{
    public string Id { get; }
    public string Name { get; }
    public string Cron { get; }
    public IJobData Data { get; }

    public RecurringJob(string id, string name, string cron, IJobData data)
    {
        Id = id;
        Name = name;
        Cron = cron;
        Data = data;
    }
}