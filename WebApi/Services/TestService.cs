namespace WebApi.Services;

using Flurl;
using Flurl.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using WebApi.Configuration;

public interface ITestService
{
    Task CallApi(
        int[] serverSideDelays,
        int? clientTimeout,
        bool useClientSideCancellationToken,
        bool useServerSideCancellationToken,
        CancellationToken cancellationToken);

    Task ExecuteSqlQuery(
        int[] delaysInSeconds,
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

    public async Task CallApi(
        int[] serverSideDelays,
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
        
        this.logger.LogInformation("Sending {Total} requests", serverSideDelays.Length);
        
        for (var i = 0; i < serverSideDelays.Length; i++)
        {
            var serverSideDelay = serverSideDelays[i];
            
            this.logger.LogInformation(
                "Sending request {Count}/{Total}, ServerDelay: {Delay} seconds, ClientTimeout: {ClientTimeout}",
                i + 1,
                serverSideDelays.Length,
                serverSideDelay,
                clientTimeout);

            url = url.AppendQueryParam("delay", serverSideDelay);

            var response = await SendRequest(
                url,
                clientTimeout,
                useClientSideCancellationToken ? cancellationToken : CancellationToken.None);

            this.logger.LogInformation(
                "Completed request {Count}/{Total}, StatusCode: {StatusCode}",
                i + 1,
                serverSideDelays.Length,
                response.StatusCode);
        }
        
        this.logger.LogInformation("Completed sending {Total} requests", serverSideDelays.Length);
    }
    
    public async Task ExecuteSqlQuery(
        int[] delaysInSeconds,
        int commandTimeoutInSeconds,
        bool throwException,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var connection = new SqlConnection(this.options.CurrentValue.SqlServer.ConnectionString);
            
            await connection.OpenAsync(cancellationToken);
            
            this.logger.LogInformation("Running {Total} queries", delaysInSeconds.Length);

            for(var i = 0; i < delaysInSeconds.Length; i++)
            {
                var delayInSeconds = delaysInSeconds[i];
                
                // Create a time string for WAITFOR DELAY in format 'hh:mm:ss'
                var delayTime = TimeSpan.FromSeconds(delayInSeconds).ToString(@"hh\:mm\:ss");
                
                var query = $"WAITFOR DELAY '{delayTime}';";
                
                await using var command = new SqlCommand(query, connection);

                command.CommandTimeout = commandTimeoutInSeconds;

                this.logger.LogInformation(
                    "Running query {Count}/{Total}, Delay: {Delay} seconds, CommandTimeout: {CommandTimeout}",
                    i + 1,
                    delaysInSeconds.Length,
                    delayInSeconds,
                    commandTimeoutInSeconds);
            
                await command.ExecuteNonQueryAsync(cancellationToken);

                this.logger.LogInformation(
                    "Completed query {Count}/{Total}",
                    i + 1,
                    delaysInSeconds.Length);
            }
            
            this.logger.LogInformation("Completed running {Total} queries", delaysInSeconds.Length);
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

    private static async Task<IFlurlResponse> SendRequest(
        Url url,
        int? clientTimeout,
        CancellationToken cancellationToken)
    {
        if (clientTimeout.HasValue)
        {
            return await url
                .WithTimeout(clientTimeout.Value)
                .GetAsync(cancellationToken: cancellationToken);
        }

        return await url.GetAsync(cancellationToken: cancellationToken);
    }
}