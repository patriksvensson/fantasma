namespace Fantasma.Sandbox.Jobs;

public sealed class FailingJob : JobHandler<FailingJob.Data>
{
    public sealed class Data : IJobData
    {
    }

    public override Task Handle(Data data, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Some kind of weird error");
    }
}