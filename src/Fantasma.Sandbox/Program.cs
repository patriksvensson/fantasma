using Fantasma.Sandbox.Components;
using Fantasma.Sandbox.Jobs;

namespace Fantasma.Sandbox;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddFantasma(config =>
        {
            config.RegisterHandlersInAssemblyContaining<Program>();
            config.AddRecurringJob("recurring", "*/10 * * * * *", new MyRecurringJob.Data());
        });

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}