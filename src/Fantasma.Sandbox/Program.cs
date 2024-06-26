using Fantasma.Sandbox.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Fantasma.Sandbox;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure the database
        builder.Services.AddDbContext<DatabaseContext>(
            options =>
            {
                options.UseSqlServer(
                    builder.Configuration
                        .GetConnectionString("Database"));
            });

        // Configure fantasma
        builder.Services.AddFantasma(
            config =>
            {
                config.RegisterHandlersInAssemblyContaining<Program>();
                config.UseEntityFramework<DatabaseContext>();
                config.NoClustering();

                // Run a job every 10 seconds
                config.AddRecurringJob(
                    "A recurring job",
                    new JobId("recurring-job"),
                    new Cron("*/10 * * * * *"),
                    new MyRecurringJob.Data(32));
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Perform database migrations
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            context.Database.Migrate();
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.MapGet(
                "/schedule", async (IJobScheduler scheduler) =>
                {
                    // Schedule a one-off job
                    var value = Random.Shared.Next();
                    var scheduled = await scheduler.Schedule(
                        $"A one-off job ({value})",
                        new MyOneOffJob.Data(value),
                        Trigger.Now);

                    return scheduled
                        ? Results.Ok()
                        : Results.StatusCode(502);
                })
            .WithName("Schedule")
            .WithOpenApi();

        app.MapGet(
                "/schedule/failing", async (IJobScheduler scheduler) =>
                {
                    // Schedule a failing one-off job
                    var scheduled = await scheduler.Schedule(
                        "A failing job",
                        new FailingJob.Data(),
                        Trigger.Now);

                    return scheduled
                        ? Results.Ok()
                        : Results.StatusCode(502);
                })
            .WithName("Schedule Failing")
            .WithOpenApi();

        app.Run();
    }
}