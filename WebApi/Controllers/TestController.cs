namespace WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Services;

[ApiController]
[Route("api/v1/[controller]")]
public class TestController(
    ITestService testService,
    ILogger<TestController> logger) : ControllerBase
{
    private readonly ITestService testService = testService;

    private readonly ILogger<TestController> logger = logger;

    [HttpGet("static-json")]
    public TestModel GetJson()
        => new()
        {
            Id = Guid.NewGuid(),
            Status = "Some status"
        };

    [HttpGet("action-result")]
    public IActionResult GetJsonActionResult()
        => this.Ok(
            new TestModel
            {
                Id = Guid.NewGuid(),
                Status = "Some status"
            });

    [HttpGet("json")]
    public List<TestModel> GetJson([FromQuery]int n)
    {
        var objects = Enumerable
            .Range(1, n)
            .Select(
                x => new TestModel
                {
                    Id = Guid.NewGuid(),
                    Status = $"Some status for object {x}"
                })
            .ToList();

        return objects;
    }

    [HttpGet("call-api")]
    public async Task<IActionResult> CallApi(
        [FromQuery(Name = "serverSideDelay")] int[] serverSideDelays,
        [FromQuery] int? clientTimeout,
        [FromQuery] bool useClientSideCancellationToken,
        [FromQuery] bool useServerSideCancellationToken,
        CancellationToken cancellationToken)
    {
        this.logger.LogInformation(
            "Handling request {Name}. ServerSideDelays: {ServerSideDelays}, ClientTimeout: {ClientTimeout}, UseClientSideCancellationToken: {UseClientSideCancellationToken}, UseServerSideCancellationToken: {UseServerSideCancellationToken}",
            nameof(this.CallApi),
            string.Join(",", serverSideDelays),
            clientTimeout.HasValue ? clientTimeout.Value.ToString() : "None",
            useClientSideCancellationToken,
            useServerSideCancellationToken);

        await this.testService.CallApi(
            serverSideDelays,
            clientTimeout,
            useClientSideCancellationToken,
            useServerSideCancellationToken,
            cancellationToken);

        this.logger.LogInformation("Completed requests");

        return this.Ok();
    }
    
    [HttpGet("sql-query")]
    public async Task<IActionResult> ExecuteSqlQuery(
        [FromQuery(Name = "queryDelayInSeconds")] int[] queryDelaysInSeconds,
        [FromQuery] int commandTimeoutInSeconds,
        [FromQuery] bool useCancellationToken,
        [FromQuery(Name = "throw")] bool throwException,
        CancellationToken cancellationToken)
    {
        this.logger.LogInformation(
            "Handling request {Name}. QueryDelays: {Delays}, CommandTimeout: {CommandTimeout}, UseCancellationToken: {UseCancellationToken}, Throw: {Throw}",
            nameof(this.ExecuteSqlQuery),
            string.Join(",", queryDelaysInSeconds),
            commandTimeoutInSeconds,
            useCancellationToken,
            throwException);

        await this.testService.ExecuteSqlQuery(
            queryDelaysInSeconds,
            commandTimeoutInSeconds,
            throwException,
            useCancellationToken ? cancellationToken : CancellationToken.None);

        this.logger.LogInformation("Completed request");

        return this.Ok();
    }

    [HttpGet("delay")]
    public async Task<IActionResult> Delay(
        [FromQuery] int? delay,
        [FromQuery] bool useCancellationToken,
        CancellationToken cancellationToken)
    {
        this.logger.LogInformation(
            "Handling request {Name}. Delay: {Delay}, UseCancellationToken: {UseCancellationToken}",
            nameof(this.Delay),
            delay.HasValue ? delay.Value.ToString() : "None",
            useCancellationToken);

        if (delay.HasValue)
        {
            cancellationToken.Register(
                () =>
                {
                    this.logger.LogInformation("CancellationToken was set");
                });

            CancellationToken cancellationTokenToUse;
            if (useCancellationToken)
            {
                cancellationTokenToUse = cancellationToken;
                this.logger.LogInformation("Used request CancellationToken");
            }
            else
            {
                cancellationTokenToUse = CancellationToken.None;
                this.logger.LogInformation("Used CancellationToken.None");
            }

            await Task.Delay(TimeSpan.FromSeconds(delay.Value), cancellationTokenToUse);
        }
        
        this.logger.LogInformation("Completed request");

        return this.Ok();
    }
}