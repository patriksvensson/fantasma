namespace Fantasma.Internal;

public sealed class InMemoryProvider : IJobProvider
{
    private readonly InMemoryStorage _storage;

    public TimeSpan Sleep { get; } = TimeSpan.FromSeconds(1);

    public InMemoryProvider(TimeProvider time)
    {
        _storage = new InMemoryStorage(time);
    }

    public IJobStorage GetStorage()
    {
        return _storage;
    }
}