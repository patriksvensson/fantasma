namespace Fantasma;

[PublicAPI]
public abstract class JobHandler<T> : IJobHandler<T>, IJobHandler
    where T : IJobData
{
    public virtual string Name { get; } = typeof(T).Name;

    public abstract Task Handle(T data, CancellationToken cancellationToken = default);

    async Task IJobHandler.Handle(IJobData data, CancellationToken cancellationToken)
    {
        if (data is not T jobData)
        {
            throw new InvalidOperationException("Could not cast job data to the correct type");
        }

        await Handle(jobData, cancellationToken);
    }
}