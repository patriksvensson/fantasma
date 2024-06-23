namespace Fantasma.Internal;

internal static class JobStorageExtensions
{
    public static async Task UpdateJob(this IJobStorage storage, Job job, Action<Job> action)
    {
        action(job);
        await storage.Update(job);
    }
}