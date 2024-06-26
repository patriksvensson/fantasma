namespace Fantasma.Sandbox.Jobs;

public sealed class MyRecurringJob : JobHandler<MyRecurringJob.Data>
{
    private readonly ILogger<MyRecurringJob> _logger;

    public MyRecurringJob(ILogger<MyRecurringJob> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public sealed class Data : IJobData
    {
        public int Value { get; init; }

        public Data(int value)
        {
            Value = value;
        }
    }

    public override Task Handle(Data data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Hello from a recurring job: {data.Value}");
        return Task.CompletedTask;
    }
}