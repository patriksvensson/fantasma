namespace Fantasma;

[PublicAPI]
public interface IJobEngine
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}