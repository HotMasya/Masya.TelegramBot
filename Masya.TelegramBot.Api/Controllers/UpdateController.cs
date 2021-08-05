using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Api.Controllers
{
    public class UpdateController : ControllerBase
    {
        private readonly IBotService _botService;

        public UpdateController(IBotService botService)
        {
            _botService = botService;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] Update update)
        {
            await _botService.HandleUpdateAsync(update);
            return Ok();
        }
    }
}
