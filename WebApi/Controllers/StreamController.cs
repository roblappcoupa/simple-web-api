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
    [HttpGet("stream")]
    public async Task StreamData(CancellationToken cancellationToken)
    {
        this.Response.ContentType = "text/plain";

        await foreach (var data in GetDataStream(cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            // Write each item to the response
            await this.Response.WriteAsync($"{data}\n", cancellationToken);
            await this.Response.Body.FlushAsync(cancellationToken); // Flush after each write to ensure data is sent immediately
        }
    }

    private static async IAsyncEnumerable<string> GetDataStream([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (var i = 0; i < 100; i++)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken); // Simulate delay
            yield return $"Data item {i + 1}";
        }
    }
}
