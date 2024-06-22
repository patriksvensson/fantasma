using Fantasma;

namespace Fantasma.Sandbox.Jobs;

public sealed class MyRecurringJob : JobHandler<MyRecurringJob.Data>
{
    private readonly ILogger<MyJobHandler> _logger;

    public MyRecurringJob(ILogger<MyJobHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public sealed class Data : IJobData
    {
    }

    public override Task Handle(Data data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"A recurring job checking in {DateTime.Now:HH:mm:ss}");
        return Task.CompletedTask;
    }
}