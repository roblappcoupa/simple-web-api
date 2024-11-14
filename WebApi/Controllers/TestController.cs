namespace WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Services;

[ApiController]
[Route("api/v1/[controller]")]
public class TestController : ControllerBase
{
    private readonly ITestService testService;
    private readonly ILogger<TestController> logger;
    
    public TestController(
        ITestService testService,
        ILogger<TestController> logger)
    {
        this.testService = testService;
        this.logger = logger;
    }
    
    [HttpGet("call-api")]
    public async Task<TestModel> CallApi(
        [FromQuery] int? serverSideDelay,
        [FromQuery] int? clientTimeout,
        [FromQuery] bool useClientSideCancellationToken,
        [FromQuery] bool useServerSideCancellationToken,
        CancellationToken cancellationToken)
    {
        this.logger.LogInformation(
            "Handling request. ServerSideDelay: {ServerSideDelay}, ClientTimeout: {ClientTimeout}, UseClientSideCancellationToken: {UseClientSideCancellationToken}, UseServerSideCancellationToken: {UseServerSideCancellationToken}",
            serverSideDelay.HasValue ? serverSideDelay.Value.ToString() : "None",
            clientTimeout.HasValue ? clientTimeout.Value.ToString() : "None",
            useClientSideCancellationToken,
            useServerSideCancellationToken);

        var result = await this.testService.CallApi(
            serverSideDelay,
            clientTimeout,
            useClientSideCancellationToken,
            useServerSideCancellationToken,
            cancellationToken);

        return result;
    }
    
    [HttpGet("sql-query")]
    public async Task<TestModel> ExecuteSqlQuery(
        [FromQuery] int queryDelayInSeconds,
        [FromQuery] int? postQueryDelayInSeconds,
        [FromQuery] int commandTimeoutInSeconds,
        [FromQuery] bool useCancellationToken,
        [FromQuery(Name = "throw")] bool throwException,
        CancellationToken cancellationToken)
    {
        this.logger.LogInformation(
            "Handling request {Name}. QueryDelay: {Delay}, PostQueryDelay: {PostQueryDelay}, CommandTimeout: {CommandTimeout}, UseCancellationToken: {UseCancellationToken}, Throw: {Throw}",
            nameof(this.ExecuteSqlQuery),
            queryDelayInSeconds,
            postQueryDelayInSeconds.HasValue ? postQueryDelayInSeconds.Value.ToString() : "NONE",
            commandTimeoutInSeconds,
            useCancellationToken,
            throwException);

        await this.testService.ExecuteSqlQuery(
            queryDelayInSeconds,
            postQueryDelayInSeconds,
            commandTimeoutInSeconds,
            throwException,
            useCancellationToken ? cancellationToken : CancellationToken.None);

        return new TestModel
        {
            Id = Guid.NewGuid(),
            Status = "Complete"
        };
    }

    [HttpGet("delay")]
    public async Task<TestModel> Delay(
        [FromQuery] int? delay,
        [FromQuery] bool useCancellationToken,
        CancellationToken cancellationToken)
    {
        this.logger.LogInformation(
            "Handling request. Delay: {Delay}, UseCancellationToken: {UseCancellationToken}",
            delay.HasValue ? delay.Value.ToString() : "None",
            useCancellationToken);

        if (delay.HasValue)
        {
            await Task.Delay(TimeSpan.FromSeconds(delay.Value), useCancellationToken ? cancellationToken : CancellationToken.None);
        }
        
        this.logger.LogInformation("Completed request");

        return new TestModel
        {
            Id = Guid.NewGuid(),
            Status = "OK"
        };
    }
}