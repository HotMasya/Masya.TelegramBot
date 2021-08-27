using System.Linq;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Api.Controllers
{
    public class BotController : ControllerBase
    {
        private readonly IBotService _botService;
        private readonly ApplicationDbContext _dbContext;

        public BotController(IBotService botService, ApplicationDbContext dbContext)
        {
            _botService = botService;
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] Update update)
        {
            var settings = _dbContext.BotSettings.First();
            var url = settings.WebhookHost.Replace("{BOT_TOKEN}", settings.BotToken);

            if (url.EndsWith(Request.Path))
            {
                await _botService.HandleUpdateAsync(update);
                return Ok();
            }

            return NotFound();
        }
    }
}
