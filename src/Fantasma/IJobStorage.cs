namespace Fantasma;

[PublicAPI]
public interface IJobStorage : IDisposable
{
    Task Add(Job job);
    Task Remove(Job job);

    Task Update(Job job);

    Task<Job?> GetNextJob();
    Task Release(Job job);
}