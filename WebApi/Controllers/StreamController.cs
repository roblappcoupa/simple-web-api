namespace WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[ApiController]
[Route("api/v1/[controller]")]
public class StreamController : ControllerBase
{
    private readonly ILogger<StreamController> logger;
    
    public StreamController(ILogger<StreamController> logger)
    {
        this.logger = logger;
    }

    [HttpGet]
    public async Task StreamData(
        [FromQuery]int lines,
        [FromQuery]int delay,
        CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Handling request. Lines: {Lines}, Delay: {Delay}", lines, delay);
        
        this.Response.ContentType = "text/plain";

        await foreach (var data in this.GetDataStream(lines, delay, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                this.logger.LogInformation("Cancellation token was set");
                break;
            }

            // Write each item to the response
            await this.Response.WriteAsync($"{data}\n", cancellationToken);
            await this.Response.Body.FlushAsync(cancellationToken); // Flush after each write to ensure data is sent immediately
        }
        
        this.logger.LogInformation("Completed request. Lines: {Lines}, Delay: {Delay}", lines, delay);
    }

    private async IAsyncEnumerable<string> GetDataStream(
        int lines,
        int delay,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (var i = 0; i < lines; i++)
        {
            await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);

            this.logger.LogInformation("Writing data item {Item}", i + 1);

            yield return $"Data item {i + 1}";
        }
    }
}
