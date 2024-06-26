namespace Fantasma.Internal;

internal sealed class ScopedStorageAdapter : IJobStorage
{
    private readonly IServiceScope _scope;
    private readonly IJobStorage _storage;

    public ScopedStorageAdapter(IServiceScope scope, IJobStorage storage)
    {
        _scope = scope;
        _storage = storage;
    }

    public void Dispose()
    {
        _storage.Dispose();
        _scope.Dispose();
    }

    public Task Add(Job job)
    {
        return _storage.Add(job);
    }

    public Task Remove(Job job)
    {
        return _storage.Remove(job);
    }

    public Task Update(Job job)
    {
        return _storage.Update(job);
    }

    public Task<Job?> GetNextJob()
    {
        return _storage.GetNextJob();
    }

    public Task Release(CompletedJob job)
    {
        return _storage.Release(job);
    }
}