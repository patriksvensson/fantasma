namespace Fantasma.Internal;

internal sealed class SqlProvider : IJobProvider
{
    private readonly IServiceProvider _services;

    public TimeSpan Sleep { get; } = TimeSpan.FromSeconds(10);

    public SqlProvider(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public IJobStorage GetStorage()
    {
        var scope = _services.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<SqlStorage>();
        return new ScopedStorageAdapter(scope, storage);
    }
}