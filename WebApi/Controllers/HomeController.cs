namespace WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
public class HomeController : ControllerBase
{
    [HttpGet("/")]
    public IActionResult Index()
        => this.PhysicalFile(
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "index.html"),
            "text/html");
}
