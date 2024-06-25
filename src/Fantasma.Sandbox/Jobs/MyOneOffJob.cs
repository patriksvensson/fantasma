namespace Fantasma.Sandbox.Jobs;

public sealed class MyOneOffJob : JobHandler<MyOneOffJob.Data>
{
    private readonly ILogger<MyOneOffJob> _logger;

    public MyOneOffJob(ILogger<MyOneOffJob> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public sealed class Data : IJobData
    {
        public int Value { get; }

        public Data(int value)
        {
            Value = value;
        }
    }

    public override Task Handle(Data data, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation($"Hello from one-off job: {data.Value}");
        return Task.CompletedTask;
    }
}