using RedditToOpml.Components;
using RedditToOpml.Data;
using Serilog;

namespace RedditToOpml;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Configure Serilog as the static logger
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/server.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            // Use Serilog for logging
            builder.Services.AddSerilog(Log.Logger);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddSingleton<Repository>();
            
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapControllers();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            await app.RunAsync();
        }
        finally
        {
            // Ensure to flush and close the logger
            await Log.CloseAndFlushAsync();
        }
    }
}