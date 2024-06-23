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
                config.SetSleepPreference(TimeSpan.FromSeconds(2));

                // Run a job every 10 seconds
                config.AddRecurringJob(
                    "recurring-job",
                    "*/10 * * * * *",
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
                    var scheduled = await scheduler.Schedule(
                        new MyOneOffJob.Data(Random.Shared.Next()),
                        Trigger.Now());

                    return scheduled
                        ? Results.Ok()
                        : Results.StatusCode(502);
                })
            .WithName("GetWeatherForecast")
            .WithOpenApi();

        app.Run();
    }
}