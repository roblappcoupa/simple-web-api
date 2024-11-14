using WebApi.Configuration;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

var configurationSection = builder.Configuration.GetSection("WebApi");
builder.Services.Configure<ApplicationConfiguration>(configurationSection);

ApplicationConfiguration config = new();
configurationSection.Bind(config);

if (config.Server.ShutDownTime.HasValue)
{
    builder.WebHost.UseShutdownTimeout(config.Server.ShutDownTime.Value);
}

builder.Services.AddControllers();

builder.Services.AddHttpLogging(
    options =>
    {
        options.RequestHeaders.Add("X-Real-IP");
        options.RequestHeaders.Add("X-Forwarded-For");
        options.RequestHeaders.Add("X-Forwarded-Proto");
    });

builder.Services.AddScoped<ITestService, TestService>();

var app = builder.Build();

app.UseHttpLogging();

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