namespace Fantasma.Internal;

internal sealed class NopCluster : ICluster
{
    public bool IsLeader { get; } = true;

    public Task Connect(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task Update(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task Disconnect(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }
}