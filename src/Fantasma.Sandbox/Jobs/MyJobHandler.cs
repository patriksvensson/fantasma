using Fantasma;

namespace Fantasma.Sandbox.Jobs;

public class MyJobHandler : JobHandler<MyJobHandler.Data>
{
    private readonly ILogger<MyJobHandler> _logger;

    public MyJobHandler(ILogger<MyJobHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public sealed class Data : IJobData
    {
        public required DateTime Timestamp { get; init; }
    }

    public override Task Handle(Data data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Hello from job handler: {Timestamp}", data.Timestamp);
        return Task.CompletedTask;
    }
}