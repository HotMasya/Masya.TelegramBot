using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Masya.TelegramBot.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class MainController : ControllerBase
    {
        private readonly ILogger<MainController> _logger;
        private readonly IBotService _botService;
        private readonly ICommandService _commandService;

        public MainController(IBotService botService, ICommandService commandService, ILogger<MainController> logger)
        {
            _botService = botService;
            _commandService = commandService;
            _logger = logger;
        }

        [HttpGet("bot")]
        public async Task<IActionResult> GetBotStatusAsync()
        {
            var status = await _botService.GetStatusAsync();
            return Ok(status);
        }
    }
}
