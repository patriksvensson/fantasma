# Fantasma

Fantasma is a minimalistic, in-memory (for now) job scheduler library for ASP.NET Core, with a MediatR-like API.

- [x] Job queuing
- [x] Delayed jobs
- [x] Recurring jobs
- [x] In-memory storage
- [ ] Health check
- [ ] SQL storage
- [ ] SQLite storage
- [ ] API endpoint

## Usage

### Registration

```csharp
services.AddFantasma(config => 
{
    // Register all job handlers within the same 
    // assembly as `Program`.
    config.RegisterHandlersInAssemblyContaining<Program>();

    // Schedule a recurring job that executes 
    // every 10 seconds.
    config.ScheduleRecurring(
        "unique-id-of-job", 
        "*/10 * * * * *", 
        new MyRecurringJob.Data());
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
        new MyJob { Foo = 32 },
        Trigger.Now);

    // Schedule job in the future
    scheduler.Schedule(
        new MyJob { Foo = 32 },
        Trigger.AtTime(
            DateTime.Now.AddMinutes(30))
    );

    // Schedule recurring job
    scheduler.Schedule(
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