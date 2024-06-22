namespace Fantasma;

[PublicAPI]
public interface IJobScheduler
{
    Task<bool> Schedule(IJobData data, Trigger trigger);
}