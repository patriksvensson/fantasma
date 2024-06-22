namespace Fantasma.Internal;

internal interface IJobHandler<in T>
    where T : IJobData
{
    Task Handle(T data, CancellationToken cancellationToken = default);
}

internal interface IJobHandler
{
    Task Handle(IJobData data, CancellationToken cancellationToken = default);
}