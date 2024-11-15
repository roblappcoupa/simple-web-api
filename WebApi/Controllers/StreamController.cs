namespace WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[ApiController]
[Route("api/v1/[controller]")]
public class StreamController(
    ILogger<StreamController> logger) : ControllerBase
{
    private readonly ILogger<StreamController> logger = logger;

    [HttpGet("sse")]
    public Task ServerSentEvents(
        [FromQuery] int lines,
        [FromQuery] int? initialDelay,
        [FromQuery] int delay,
        [FromQuery] bool useCancellationToken,
        [FromQuery(Name = "throw")] bool throwException,
        CancellationToken cancellationToken)
        => this.StreamHelper(
            lines,
            initialDelay,
            delay,
            useCancellationToken,
            throwException,
            "text/event-stream",
            cancellationToken);

    [HttpGet]
    public Task StreamData(
        [FromQuery] int lines,
        [FromQuery] int? initialDelay,
        [FromQuery] int delay,
        [FromQuery] bool useCancellationToken,
        [FromQuery(Name = "throw")] bool throwException,
        CancellationToken cancellationToken)
        => this.StreamHelper(
            lines,
            initialDelay,
            delay,
            useCancellationToken,
            throwException,
            "text/plain",
            cancellationToken);

    private async Task StreamHelper(
        int lines,
        int? initialDelay,
        int delay,
        bool useCancellationToken,
        bool throwException,
        string contentType,
        CancellationToken cancellationToken)
    {
        try
        {
            var cancellationTokenToUse = useCancellationToken ? cancellationToken : CancellationToken.None;
            
            this.logger.LogInformation(
                "Handling request. Lines: {Lines}, Delay: {Delay}, Throw: {Throw}, UseCancellationToken: {UseCancellationToken}",
                lines,
                delay,
                throwException,
                useCancellationToken);
            
            // Delay before ANY part of the response has been read
            if (initialDelay.HasValue)
            {
                await Task.Delay(TimeSpan.FromSeconds(initialDelay.Value), cancellationTokenToUse);
            }

            this.Response.ContentType = contentType;
            this.Response.Headers.CacheControl = "no-cache";
            // this.Response.Headers["X-Accel-Buffering"] = "no";


            await foreach (var data in this.GetDataStream(lines, delay, cancellationTokenToUse))
            {
                if (cancellationTokenToUse.IsCancellationRequested)
                {
                    this.logger.LogInformation("Cancellation token was set");

                    break;
                }

                // Write each item to the response
                await this.Response.WriteAsync($"data: {data}\n\n", cancellationTokenToUse); // SSE's require 2 newlines
                await this.Response.Body.FlushAsync(cancellationTokenToUse); // Flush after each write to ensure data is sent immediately
            }

            this.logger.LogInformation("Completed request. Lines: {Lines}, Delay: {Delay}", lines, delay);
        }
        catch (Exception exception)
        {
            this.logger.LogWarning(exception, "An Exception was thrown");

            if (throwException)
            {
                throw;
            }
        }
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
