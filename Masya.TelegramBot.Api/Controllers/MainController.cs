using System.Linq;
using System.Threading.Tasks;
using Coravel.Queuing.Interfaces;
using Masya.TelegramBot.Api.Dtos;
using Masya.TelegramBot.Api.Services;
using Masya.TelegramBot.Api.Services.Abstractions;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Models;
using Masya.TelegramBot.DatabaseExtensions.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;

namespace Masya.TelegramBot.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class MainController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IBotService<DatabaseCommandInfo, DatabaseAliasInfo> _botService;
        private readonly IDatabaseLogsService _logs;
        private readonly IQueue _queue;


        public MainController(
            IBotService<DatabaseCommandInfo, DatabaseAliasInfo> botService,
            ApplicationDbContext dbContext,
            IDatabaseLogsService logs,
            IQueue queue
        )
        {
            _botService = botService;
            _dbContext = dbContext;
            _logs = logs;
            _queue = queue;
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetBotLogs()
        {
            return Ok(await _logs.GetBotLogsForLastHourAsync());
        }

        [HttpGet("imports/start")]
        public IActionResult StartImportsAsync()
        {
            _queue.QueueInvocable<UpdateXmlImportsInvokable>();
            return Ok();
        }

        [HttpGet("bot")]
        public async Task<IActionResult> GetBotSettings()
        {
            var settings = _dbContext.BotSettings.First();
            var me = await _botService.Client.GetMeAsync();
            var dto = new BotSettingsDto()
            {
                Id = settings.Id,
                Token = User.HasPermission(Permission.SuperAdmin) ? settings.BotToken : null,
                WebhookHost = User.HasPermission(Permission.SuperAdmin) ? settings.WebhookHost : null,
                IsEnabled = settings.IsEnabled,
                BotUser = me,
                IsImporting = settings.IsImporting,
            };
            return Ok(dto);
        }

        [HttpPost("bot/update")]
        public async Task<IActionResult> UpdateBotSettingsAsync([FromBody] BotSettingsDto dto)
        {
            if (!User.HasPermission(Permission.SuperAdmin)) return Forbid();

            var settings = _dbContext.BotSettings.FirstOrDefault(bs => bs.Id == dto.Id);
            if (settings is null)
            {
                return BadRequest(new MessageResponseDto("Unable to update settings. Setting not found."));
            }

            bool testResult = await _botService.TestSettingsAsync(dto.Token, dto.WebhookHost.Replace("{BOT_TOKEN}", dto.Token));
            if (!testResult)
            {
                return BadRequest(new MessageResponseDto("Invalid bot token or webhook host."));
            }

            settings.BotToken = dto.Token;
            settings.WebhookHost = dto.WebhookHost;
            settings.IsEnabled = dto.IsEnabled ?? settings.IsEnabled;
            await _dbContext.SaveChangesAsync();
            _botService.LoadBot();
            return Ok();
        }
    }
}
