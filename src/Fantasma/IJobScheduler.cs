namespace Fantasma;

[PublicAPI]
public interface IJobScheduler
{
    Task<bool> Schedule(string name, IJobData data, Trigger trigger);
}