using Microsoft.AspNetCore.Mvc;

namespace Masya.TelegramBot.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class MainController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new { Message = "Hello from main controller.", Count = 1 });
        }
    }
}
