using Microsoft.AspNetCore.Mvc;
namespace store.WebAPI.Controllers;

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