using System.Diagnostics;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;
using WebApi.Configuration;
using WebApi.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(
        new ExpressionTemplate(
            "{@t:yyyy-MM-dd HH:mm:ss.fff} [{@l}] {#if SourceContext is not null}[{SourceContext}]{#end} {@m}{#if @x is not null}\n{@p}{#end}\n{@x}\n",
            theme: TemplateTheme.Code
        ))
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

var configurationSection = builder.Configuration.GetSection("WebApi");
builder.Services.Configure<ApplicationConfiguration>(configurationSection);

ApplicationConfiguration config = new();
configurationSection.Bind(config);

builder.Services.AddSerilog(
    x =>
    {
        x.MinimumLevel.Warning()
            .ReadFrom.Configuration(builder.Configuration);
    });

if (config.Server.ShutDownTime.HasValue)
{
    Log.Logger.Information("Using non-default shut down time of {ShutDownTime}", config.Server.ShutDownTime.Value.ToString("g"));
    builder.WebHost.UseShutdownTimeout(config.Server.ShutDownTime.Value);
}

builder.Services.AddControllers();

// FOR REQUEST LOGGING
// builder.Services.AddHttpLogging(
//     options =>
//     {
//         options.RequestHeaders.Add("X-Real-IP");
//         options.RequestHeaders.Add("X-Forwarded-For");
//         options.RequestHeaders.Add("X-Forwarded-Proto");
//     });

builder.Services.AddScoped<ITestService, TestService>();

var app = builder.Build();

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

// FOR REQUEST LOGGING
// app.UseHttpLogging();
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();