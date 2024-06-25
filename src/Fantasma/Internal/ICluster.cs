namespace Fantasma.Internal;

internal interface ICluster : IDisposable
{
    bool IsLeader { get; }

    Task Connect(CancellationToken cancellationToken);
    Task Update(CancellationToken cancellationToken);
    Task Disconnect(CancellationToken cancellationToken);
}