using Microsoft.AspNetCore.Mvc;

namespace Masya.TelegramBot.Api.Controllers
{
    [ApiController]
    [Route("api/test/[controller]/[action]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new { Message = "This is a test controller" });
        }
    }
}
