using System.Linq;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DatabaseExtensions.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Api.Controllers
{
    public class BotController : ControllerBase
    {
        private readonly IBotService<DatabaseCommandInfo, DatabaseAliasInfo> _botService;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<BotController> _logger;

        public BotController(
            IBotService<DatabaseCommandInfo, DatabaseAliasInfo> botService,
            ApplicationDbContext dbContext,
            ILogger<BotController> logger)
        {
            _botService = botService;
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] Update update)
        {
            var settings = _dbContext.BotSettings.First();
            var url = settings.WebhookHost.Replace("{BOT_TOKEN}", settings.BotToken);

            if (url.EndsWith(Request.Path))
            {
                await _botService.HandleUpdateAsync(update);
            }

            return Ok();
        }
    }
}
