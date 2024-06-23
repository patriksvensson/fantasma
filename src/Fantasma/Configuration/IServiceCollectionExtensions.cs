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
        services.AddSingleton<TimeProvider>(_ => TimeProvider.System);

        if (configuration.DatabaseContext != null)
        {
            // Entity Framework storage
            services.AddSingleton<IJobProvider, SqlProvider>();
            services.AddSingleton<ICluster, SqlCluster>();
            services.AddSingleton<SqlHeartbeatThread>();
            services.AddScoped<SqlStorage>();
            services.AddScoped(typeof(IFantasmaDatabase), configuration.DatabaseContext);
        }
        else
        {
            // In-memory storage
            services.AddSingleton<IJobProvider, InMemoryProvider>();
            services.AddSingleton<ICluster, NopCluster>();
        }

        // No clustering?
        if (configuration.NoCluster)
        {
            services.Remove<ICluster>();
            services.AddSingleton<ICluster, NopCluster>();
        }

        // Got sleep preference?
        if (configuration.SleepPreference != null)
        {
            services.AddSingleton(
                new SleepPreference(
                    configuration.SleepPreference.Value));
        }

        // Register all recurring jobs
        foreach (var job in configuration.RecurringJobs)
        {
            services.AddTransient(_ => job);
        }

        return services;
    }
}