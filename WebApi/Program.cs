using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;
using WebApi.Configuration;
using WebApi.Services;
using WebApi.Utils;

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

builder.WebHost.ConfigureKestrel(
    kestrelServerOptions =>
    {
        Log.Logger.Information("Kestrel keep-alive timeout: {KeepAliveTimeout}", kestrelServerOptions.Limits.KeepAliveTimeout.ToString("g"));
    });

ApplicationConfiguration config = new();
configurationSection.Bind(config);

builder.Services.AddSerilog(
    x =>
    {
        x.MinimumLevel.Warning()
            .ReadFrom.Configuration(builder.Configuration);
    });

builder.Services.AddControllers();

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            "AllowAllPolicy",
            corsPolicyBuilder =>
            {
                corsPolicyBuilder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
    });

// FOR ALTERNATIVE REQUEST LOGGING
builder.Services.AddHttpLogging(
    options =>
    {
        options.RequestHeaders.Add("X-Real-IP");
        options.RequestHeaders.Add("X-Forwarded-For");
        options.RequestHeaders.Add("X-Forwarded-Proto");
    });

builder.Services.AddScoped<ITestService, TestService>();

if (config.Server.ShutDownTime.HasValue)
{
    Log.Logger.Information("Using non-default shut down time of {ShutDownTime}", config.Server.ShutDownTime.Value.ToString("g"));
    builder.WebHost.UseShutdownTimeout(config.Server.ShutDownTime.Value);
}

var app = builder.Build();

app.AddSerilogHostingLifetimeEventLogging();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// FOR ALTERNATIVE REQUEST LOGGING
app.UseHttpLogging();

// app.UseSerilogRequestLogging();

app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAllPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();