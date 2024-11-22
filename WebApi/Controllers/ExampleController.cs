namespace WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/v1/[controller]")]
public class ExampleController : ControllerBase
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILogger<ExampleController> logger;
    
    public ExampleController(
        IHttpContextAccessor httpContextAccessor,
        ILogger<ExampleController> logger)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.logger = logger;
    }

    [HttpGet("method1")]
    public IActionResult CancelMethod1(CancellationToken cancellationToken)
    {
        // Now use cancellationToken
        
        return this.Ok();
    }
    
    [HttpGet("method2")]
    public IActionResult CancelMethod2()
    {
        var cancellationToken = this.HttpContext.RequestAborted;
        
        // Now use cancellationToken
        
        return this.Ok();
    }
    
    [HttpGet("method3")]
    public IActionResult CancelMethod3()
    {
        var cancellationToken = this.httpContextAccessor.HttpContext?.RequestAborted;
        
        // Now use cancellationToken
        
        return this.Ok();
    }

    private async Task SomeLongRunningMethod(CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
    }
}