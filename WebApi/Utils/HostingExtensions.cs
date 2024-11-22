namespace WebApi.Utils;

using System.Diagnostics;
using Serilog;

internal static class HostingExtensions
{
    public static void AddSerilogHostingLifetimeEventLogging(this WebApplication app)
    {
        app.Lifetime.ApplicationStarted.Register(
            () =>
            {
                var process = Process.GetCurrentProcess();

                Log.Logger.Information(
                    "Started application. ProcessName: {ProcessName}, ProcessId: {ProcessId}",
                    process.ProcessName,
                    process.Id);
            });

        app.Lifetime.ApplicationStopping.Register(
            () =>
            {
                Log.Logger.Information("Stopping application");
            });

        app.Lifetime.ApplicationStopped.Register(
            () =>
            {
                Log.Logger.Information("Stopped application");
            });
    }
}