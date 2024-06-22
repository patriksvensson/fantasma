using Fantasma.Internal;

namespace Fantasma;

[PublicAPI]
public sealed class FantasmaConfiguration
{
    internal List<Assembly> Assemblies { get; } = new();
    internal List<RecurringJob> RecurringJobs { get; } = new();

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

    public FantasmaConfiguration AddRecurringJob<T>(string id, string cron, T data)
        where T : IJobData
    {
        RecurringJobs.Add(new RecurringJob(id, cron, data));
        return this;
    }
}