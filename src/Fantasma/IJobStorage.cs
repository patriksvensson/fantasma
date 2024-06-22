namespace Fantasma;

[PublicAPI]
public interface IJobStorage
{
    Task Add(Job job);
    Task Remove(Job job);

    Task<Job?> Acquire();
    Task Release(Job job);
}