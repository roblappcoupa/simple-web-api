namespace WebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class TestController : ControllerBase
{
    [Authorize(AuthenticationSchemes = AuthConstants.Scheme2)]
    [HttpGet("endpoint1")]
    public IActionResult Endpoint1() => this.Ok(this.GetIdentity());
    
    [HttpGet("endpoint2")]
    public IActionResult Endpoint2() => this.Ok(this.GetIdentity());
    
    [AllowAnonymous]
    [HttpGet("endpoint3")]
    public IActionResult Endpoint3() => this.Ok(this.GetIdentity());

    private string GetIdentity() => this.User.Identity?.Name ?? "NONE";

    // [HttpPost("pub")]
    // public IActionResult Publish([FromBody]PublishMessageRequest request, [FromQuery]bool convert)
    // {
    //     if (convert)
    //     {
    //         var jObj = JObject.Parse(request.Message?.ToString() ?? "{}");
    //         var json = JsonConvert.SerializeObject(new MessageWrapper<JObject>(jObj), Formatting.Indented);
    //         
    //         Console.WriteLine(json);
    //
    //         return this.Ok(json);
    //     }
    //     
    //     var str = request.Message.ToString();
    //
    //     Console.WriteLine(str);
    //
    //     return this.Ok(str);
    // }
}