using System.Linq;
using System.Threading.Tasks;
using Masya.TelegramBot.Api.Dtos;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace Masya.TelegramBot.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class MainController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IBotService _botService;

        public MainController(
            IBotService botService,
            ApplicationDbContext dbContext)
        {
            _botService = botService;
            _dbContext = dbContext;
        }

        [HttpGet("bot")]
        public IActionResult GetBotSettings()
        {
            var settings = _dbContext.BotSettings.First();
            var dto = new BotSettingsDto()
            {
                Id = settings.Id,
                Token = settings.BotToken,
                WebhookHost = settings.WebhookHost,
                IsEnabled = settings.IsEnabled,
            };
            return Ok(dto);
        }

        [HttpPost("bot/update")]
        public async Task<IActionResult> UpdateBotSettingsAsync(BotSettingsDto dto)
        {
            var settings = _dbContext.BotSettings.FirstOrDefault(bs => bs.Id == dto.Id);
            if (settings is null)
            {
                return BadRequest(new MessageResponseDto("Unable to update settings. Setting not found."));
            }

            bool testResult = await _botService.TestSettingsAsync(settings.BotToken, settings.WebhookHost);
            if (!testResult)
            {
                return BadRequest(new MessageResponseDto("Invalid bot token or webhook host."));
            }

            settings.BotToken = dto.Token;
            settings.WebhookHost = dto.WebhookHost;
            settings.IsEnabled = dto.IsEnabled ?? settings.IsEnabled;
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
