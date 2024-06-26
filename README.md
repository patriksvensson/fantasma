# Fantasma

```
/fanˈtazma/ (italian)

MASCULINE NOUN  
  (spettro) ghost * spectre (literaly) * phantom (literaly)

ADJECTIVE
  governo fantasma: shadow cabinet
  città/scrittore fantasma: ghost town/writer
```

---

Fantasma is a minimalistic, opinionated job scheduler library for ASP.NET Core, with a MediatR-like API.

- [x] Job queuing
- [x] Delayed jobs
- [x] Recurring jobs
- [x] In-memory storage
- [ ] Health checks
- [X] SQL storage
- [X] SQLite storage
- [ ] API endpoint
- [X] Clustering

## Disclaimer

> [!CAUTION]
> Read this before using Fantasma.

* **Fantasma is not yet suitable for production.**
* Database schema changes frequently, so make sure you use
  a separate database for Fantasma jobs until things are a bit more stable.
* Time sensitive jobs are not supported.  
  For example, when using the SQL backend, cluster nodes sometimes need 
  to elect a new leader (if the current leader drops off), and this can take up to a minute. During that time no jobs are processed. 
  This is by design and not something that will change.

## Usage

### Registration

```csharp
services.AddFantasma(config => 
{
    // Register all job handlers within the same assembly as `Program`.
    config.RegisterHandlersInAssemblyContaining<Program>();

    // Use Entity Framework as storage.
    // Uncomment this to use the in-memory storage. 
    config.UseEntityFramework<DatabaseContext>();

    // Turn off clustering when using Entity Framework.
    // Only do this if you only have one web server running
    // the scheduler.
    config.NoClustering();

    // Override the sleep interval between jobs.
    // The in memory storage waits 1 second between
    // job batches, but the default for SQL (EF Core) 
    // is 10 seconds to avoid hammering the database.
    config.SetSleepPreference(TimeSpan.FromSeconds(2));

    // Schedule a recurring job that executes every 10 seconds.
    config.AddRecurringJob(
        "A recurring job",
        new JobId("unique-id-of-job"),
        new Cron("*/10 * * * * *"),
        new MyRecurringJob.Data 
        {
            Foo = 32,
        });
});
```

### Jobs

```csharp
public sealed class MyJob : IJobData
{
    public int Foo { get; set; }
}
```

### Job handlers

```csharp
public sealed class MyHandler : JobHandler<MyJob>
{
    public Task Handle(MyJob data, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
```

### Scheduling

```csharp
public void DoMagic(IScheduler scheduler)
{
    // Schedule job immediately
    scheduler.Schedule(
        "A one-off job (foo is 32)",
        new MyJob { Foo = 32 },
        Trigger.Now);

    // Schedule job in the future
    scheduler.Schedule(
        "A one-off, delayed job (foo is 32)",
        new MyJob { Foo = 32 },
        Trigger.AtTime(
            DateTime.Now.AddMinutes(30))
    );

    // Schedule recurring job
    scheduler.Schedule(
        "A recurring job (foo is 32)"
        new MyJob { Foo = 32 },
        Trigger.Recurring(
            "unique-id-of-job",
            "0 0 0,6,12,18 * * *");
    );
}
```

## Building

We're using [Cake](https://github.com/cake-build/cake) as a 
[dotnet tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) 
for building. So make sure that you've restored Cake by running 
the following in the repository root:

```
> dotnet tool restore
```

After that, running the build is as easy as writing:

```
> dotnet cake
```

## Copyright

Copyright Patrik Svensson