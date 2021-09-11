using System.Threading.Tasks;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Masya.TelegramBot.Modules;
using Microsoft.Extensions.Logging;
using System.Linq;
using Masya.TelegramBot.Api.Dtos;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.DatabaseExtensions.Metadata;

namespace Masya.TelegramBot.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public sealed class CommandsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICommandService<DatabaseCommandInfo, DatabaseAliasInfo> _commands;
        private readonly ILogger<CommandsController> _logger;

        public CommandsController(
            ApplicationDbContext dbContext,
            ICommandService<DatabaseCommandInfo, DatabaseAliasInfo> commands,
            ILogger<CommandsController> logger
        )
        {
            _dbContext = dbContext;
            _commands = commands;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!User.HasPermission(Permission.SuperAdmin)) return Forbid();

            var commands = await _dbContext.Commands.ToListAsync();
            return Ok(commands);
        }

        [HttpPost("reload")]
        public async Task<IActionResult> ReloadCommandsAsync()
        {
            if (!User.HasPermission(Permission.SuperAdmin)) return Forbid();

            _logger.LogInformation("Reloading commands and aliases...");
            await _commands.LoadCommandsAsync(typeof(BasicModule).Assembly);
            _logger.LogInformation("Reloaded all commands and aliases.");
            return Ok();
        }

        [HttpPut("save")]
        public async Task<IActionResult> SaveCommandsAsync(Command[] commands)
        {
            if (!User.HasPermission(Permission.SuperAdmin)) return Forbid();

            _logger.LogInformation("Received a request to update commands and aliases.");
            var distinctNames = commands.Select(c => c.Name.ToLower()).Distinct().ToArray();
            if (distinctNames.Length != commands.Length)
            {
                return BadRequest(new MessageResponseDto("Every command or alias name should be unique."));
            }

            if (distinctNames.Any(n => n.Contains(' ')))
            {
                return BadRequest(new MessageResponseDto("The command or alias name should not contain any whitespaces."));
            }

            var dbCommands = await _dbContext.Commands.ToListAsync();
            var commandsToDelete = dbCommands.Except(commands);
            _dbContext.Commands.RemoveRange(commandsToDelete);
            _dbContext.Commands.UpdateRange(commands);
            await _dbContext.SaveChangesAsync();
            await _commands.LoadCommandsAsync(typeof(BasicModule).Assembly);
            _logger.LogInformation("Updated commands and reloaded the command service.");
            return Ok();
        }
    }
}