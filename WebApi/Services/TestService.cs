namespace WebApi.Services;

using Flurl;
using Flurl.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using WebApi.Configuration;
using WebApi.Models;

public interface ITestService
{
    Task<TestModel> CallApi(
        int? serverSideDelay,
        int? clientTimeout,
        bool useClientSideCancellationToken,
        bool useServerSideCancellationToken,
        CancellationToken cancellationToken);

    Task ExecuteSqlQuery(
        int delayInSeconds,
        int? postQueryDelayInSeconds,
        int commandTimeoutInSeconds,
        bool throwException,
        CancellationToken cancellationToken);
}

internal sealed class TestService : ITestService
{
    private readonly IOptionsMonitor<ApplicationConfiguration> options;
    private readonly ILogger<TestService> logger;

    public TestService(IOptionsMonitor<ApplicationConfiguration> options, ILogger<TestService> logger)
    {
        this.options = options;
        this.logger = logger;
    }

    public async Task<TestModel> CallApi(
        int? serverSideDelay,
        int? clientTimeout,
        bool useClientSideCancellationToken,
        bool useServerSideCancellationToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(this.options.CurrentValue.Api.ApiBaseUrl))
        {
            throw new Exception($"Configuration setting {nameof(this.options.CurrentValue.Api.ApiBaseUrl)} is not defined");
        }

        var url = this.options.CurrentValue.Api.ApiBaseUrl
            .AppendPathSegment("api/v1/test/delay")
            .AppendQueryParam("useCancellationToken", useServerSideCancellationToken);

        if (serverSideDelay.HasValue)
        {
            url = url.AppendQueryParam("delay", serverSideDelay.Value);
        }
        
        if (clientTimeout.HasValue)
        {
            return await url
                .WithTimeout(clientTimeout.Value)
                .GetJsonAsync<TestModel>(
                    cancellationToken: useClientSideCancellationToken ? cancellationToken : CancellationToken.None);
        }

        var result = await url.GetJsonAsync<TestModel>(
            cancellationToken: useClientSideCancellationToken ? cancellationToken : CancellationToken.None);

        return result;
    }
    
    public async Task ExecuteSqlQuery(
        int delayInSeconds,
        int? postQueryDelayInSeconds,
        int commandTimeoutInSeconds,
        bool throwException,
        CancellationToken cancellationToken)
    {
        // Create a time string for WAITFOR DELAY in format 'hh:mm:ss'
        var delayTime = TimeSpan.FromSeconds(delayInSeconds).ToString(@"hh\:mm\:ss");

        // Define the query with the delay
        var query = $"WAITFOR DELAY '{delayTime}';";

        // Set up your SQL connection and command
        await using var connection = new SqlConnection(this.options.CurrentValue.SqlServer.ConnectionString);
        await using var command = new SqlCommand(query, connection);

        command.CommandTimeout = commandTimeoutInSeconds;

        try
        {
            await connection.OpenAsync(cancellationToken);

            this.logger.LogInformation(
                "Sql delay: {Delay} seconds, Command timeout: {CommandTimeout}",
                delayInSeconds,
                commandTimeoutInSeconds);
            
            await command.ExecuteNonQueryAsync(cancellationToken);

            this.logger.LogInformation(
                "Completed query");

            if (postQueryDelayInSeconds == null)
            {
                return;
            }
            
            this.logger.LogInformation(
                "Delaying for {Seconds} seconds", postQueryDelayInSeconds.Value);

            await Task.Delay(TimeSpan.FromSeconds(postQueryDelayInSeconds.Value), cancellationToken);
            
            this.logger.LogInformation(
                "Completed delaying for {Seconds} seconds", postQueryDelayInSeconds.Value);
        }
        catch (Exception exception)
        {
            this.logger.LogError(
                exception,
                "An Exception was thrown invoking the SQL query");

            if (throwException)
            {
                throw;
            }
        }
    }
}