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
}