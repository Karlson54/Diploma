using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TimeTracker.API.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TestController : Controller
{
    [HttpGet]
    public IActionResult GetSmth()
    {
        return Ok(new List<object>
        {
            new
            {
                Name = "#1 test",
                Duration = TimeSpan.FromHours(2)
            },
            new
            {
                Name = "#2 test",
                Duration = TimeSpan.FromHours(5)
            }
        });
    }
}