using Microsoft.EntityFrameworkCore;

namespace Fantasma;

[PublicAPI]
public sealed class FantasmaConfiguration
{
    internal List<Assembly> Assemblies { get; } = new();
    internal List<RecurringJob> RecurringJobs { get; } = new();
    internal Type? DatabaseContext { get; set; }
    internal bool NoCluster { get; set; }
    internal TimeSpan? SleepPreference { get; set; }

    public FantasmaConfiguration RegisterHandlersInAssembly(Assembly assembly)
    {
        Assemblies.Add(assembly);
        return this;
    }

    public FantasmaConfiguration RegisterHandlersInAssemblyContaining<T>()
    {
        Assemblies.Add(typeof(T).Assembly);
        return this;
    }

    public FantasmaConfiguration UseEntityFramework<T>()
        where T : DbContext, IFantasmaDatabase
    {
        DatabaseContext = typeof(T);
        return this;
    }

    public FantasmaConfiguration NoClustering()
    {
        NoCluster = true;
        return this;
    }

    public FantasmaConfiguration SetSleepPreference(TimeSpan time)
    {
        SleepPreference = time;
        return this;
    }

    public FantasmaConfiguration AddRecurringJob<T>(string name, JobId id, Cron cron, T data)
        where T : IJobData
    {
        RecurringJobs.Add(new RecurringJob(id.Id, name, cron.Expression, data));
        return this;
    }
}