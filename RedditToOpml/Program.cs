using System.Runtime.InteropServices;
using RedditToOpml.Components;
using RedditToOpml.Data;
using Serilog;
using Serilog.Events;

namespace RedditToOpml;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSerilog(Log.Logger);
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

        ConfigureLogger(builder, app);
        try
        {
            await app.RunAsync();
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static void ConfigureLogger(WebApplicationBuilder builder, WebApplication app)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Warning)
            .MinimumLevel.Override("AssetPatchCompliance.Web.Helpers.BasicAuthenticationHandler", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.PersistentFile("logs/server.log", preserveLogFilename: true, persistentFileRollingInterval: PersistentFileRollingInterval.Day)
            .CreateLogger();

        Log.Information("---------------------------------------------");
        Log.Information("RedditToOpml");
        Log.Information("---------------------------------------------");
        Log.Information("URLs: {urls}", builder.WebHost.GetSetting(WebHostDefaults.ServerUrlsKey)?.Replace(";", " "));
        Log.Information("Runtime: {runtime}", RuntimeInformation.FrameworkDescription);
        Log.Information("Platform: {platform}", RuntimeInformation.OSDescription);
        Log.Information("Environment: {environment}", app.Environment.EnvironmentName);
        Log.Information("---------------------------------------------");
    }
}