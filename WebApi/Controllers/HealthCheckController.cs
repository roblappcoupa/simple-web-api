namespace WebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[AllowAnonymous]
[ApiController]
[Route("api/v1/[controller]")]
public class HealthCheckController : ControllerBase
{
    [HttpGet("~/health")]
    public IActionResult PerformCheck() => this.Ok("Healthy");

    [HttpGet("~/connection")]
    public IActionResult ConnectionInfo()
    {
        var connection = this.HttpContext.Connection;

        var headers = this.HttpContext.Request.Headers
            .Select(x => $"{x.Key}: {x.Value.ToString()}")
            .ToList();

        var obj = new
        {
            RemoteIpAddress = connection.RemoteIpAddress?.ToString(),
            connection.RemotePort,
            LocalIpAddress = connection.LocalIpAddress?.ToString(),
            connection.LocalPort,
            RequestHeaders = headers,
            this.HttpContext.Request.Protocol
        };

        return this.Ok(obj);
    }
}