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
    public async Task ServerSentEvents(
        [FromQuery] int lines,
        [FromQuery] int delay,
        [FromQuery] bool useCancellationToken,
        [FromQuery(Name = "throw")] bool throwException,
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

            this.Response.ContentType = "text/event-stream";
            this.Response.Headers.CacheControl = "no-cache";
            // this.Response.Headers["X-Accel-Buffering"] = "no";

            await foreach (var data in this.GetDataStream(lines, delay, cancellationTokenToUse))
            {
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

    [HttpGet]
    public async Task StreamData(
        [FromQuery] int lines,
        [FromQuery] int? initialDelay,
        [FromQuery] int delay,
        [FromQuery] bool useCancellationToken,
        [FromQuery(Name = "throw")] bool throwException,
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

            this.Response.ContentType = "text/plain";
            this.Response.Headers.CacheControl = "no-cache";
            // this.Response.Headers["X-Accel-Buffering"] = "no";

            await foreach (var data in this.GetDataStream(lines, delay, cancellationTokenToUse))
            {
                await this.Response.WriteAsync($"{data}\n", cancellationTokenToUse);
                await this.Response.Body.FlushAsync(cancellationTokenToUse);
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
