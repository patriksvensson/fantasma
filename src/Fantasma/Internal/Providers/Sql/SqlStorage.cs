using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Fantasma.Internal;

internal sealed class SqlStorage : IJobStorage
{
    private readonly IFantasmaDatabase _database;
    private readonly TimeProvider _time;
    private readonly ILogger<SqlStorage> _logger;

    public SqlStorage(
        IFantasmaDatabase database,
        TimeProvider time,
        ILogger<SqlStorage> logger)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _time = time ?? throw new ArgumentNullException(nameof(time));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Dispose()
    {
    }

    public async Task Add(Job job)
    {
        if (await _database.Jobs.FindAsync(job.Id) != null)
        {
            return;
        }

        _database.Jobs.Add(
            new FantasmaJob
                {
                    Id = job.Id,
                    ClrType = job.Data.GetType().AssemblyQualifiedName ?? throw new InvalidOperationException(),
                    Data = JsonConvert.SerializeObject(job.Data),
                    ScheduledAt = job.ScheduledAt.UtcDateTime,
                    Status = JobStatus.Scheduled,
                    Cron = job.Cron,
                });

        await _database.SaveChangesAsync();
    }

    public async Task Remove(Job job)
    {
        var deleted = await _database.Jobs.Where(j => j.Id == job.Id).ExecuteDeleteAsync();
        Debug.Assert(deleted == 1, "Expected the job to be deleted");
    }

    public async Task Update(Job job)
    {
        var dbJob = await _database.Jobs.FindAsync(job.Id);
        if (dbJob != null)
        {
            dbJob.Status = job.Status;
            dbJob.ScheduledAt = job.ScheduledAt.UtcDateTime;
            await _database.SaveChangesAsync();
        }
    }

    public Task<Job?> GetNextJob()
    {
        var now = _time.GetUtcNow();
        var nextJob = _database.Jobs.AsNoTracking()
            .Where(x => x.ScheduledAt < now)
            .AsEnumerable().MinBy(x => x.ScheduledAt);

        if (nextJob == null)
        {
            return Task.FromResult<Job?>(null);
        }

        var type = Type.GetType(nextJob.ClrType);
        if (type == null)
        {
            _logger.LogError("Could not get CLR type for job");
            return Task.FromResult<Job?>(null);
        }

        if (JsonConvert.DeserializeObject(nextJob.Data, type) is not IJobData data)
        {
            _logger.LogError("Could not deserialize job data");
            return Task.FromResult<Job?>(null);
        }

        return Task.FromResult<Job?>(
            new Job
                {
                    Id = nextJob.Id,
                    ScheduledAt = nextJob.ScheduledAt,
                    Status = JobStatus.Scheduled,
                    Data = data,
                    Cron = nextJob.Cron,
                });
    }

    public async Task Release(Job job)
    {
        // Delete the job
        var deleted = await _database.Jobs.Where(j => j.Id == job.Id).ExecuteDeleteAsync();
        Debug.Assert(deleted == 1, "Expected the job to be deleted");

        if (job.Cron != null)
        {
            _logger.LogDebug("Rescheduling recurring job");

            // Reschedule the job
            var rescheduled = job.Reschedule(_time);
            if (rescheduled != null)
            {
                await Add(rescheduled);
            }
        }
    }
}