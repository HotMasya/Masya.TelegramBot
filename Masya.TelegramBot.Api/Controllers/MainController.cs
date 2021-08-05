using Masya.TelegramBot.Commands.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Masya.TelegramBot.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
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

        [HttpGet]
        public IActionResult Index()
        {
            return Ok(
                new
                {
                    Host = _botService.Options.WebhookHost,
                    Token = _botService.Options.Token,
                    TotalCommands = _commandService.Commands.Count
                }
            );
        }
    }
}
