using Fantasma.Internal;

namespace Fantasma;

[PublicAPI]
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddFantasma(this IServiceCollection services, Action<FantasmaConfiguration> configuration)
    {
        var obj = new FantasmaConfiguration();
        configuration(obj);
        return AddFantasma(services, obj);
    }

    public static IServiceCollection AddFantasma(this IServiceCollection services, FantasmaConfiguration configuration)
    {
        // Register types in assemblies
        ServiceRegistrar.RegisterAssemblies(services, configuration);

        // Register required stuff
        services.AddHostedService<JobEngine>();
        services.AddSingleton<IJobScheduler, JobScheduler>();
        services.AddSingleton<IJobEngine, JobEngine>();
        services.AddSingleton<IJobStorage, InMemoryStorage>();
        services.AddSingleton<TimeProvider>(_ => TimeProvider.System);

        // Register all recurring jobs
        foreach (var job in configuration.RecurringJobs)
        {
            services.AddTransient(_ => job);
        }

        return services;
    }
}