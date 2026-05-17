using Microsoft.AspNetCore.Mvc;
namespace Store.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]

public class GreetingController : ControllerBase
{
    [HttpGet]
    public IActionResult Greeting()
    {
        return Ok("Hello World");
    }
}