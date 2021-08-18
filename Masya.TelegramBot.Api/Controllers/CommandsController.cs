using System.Threading.Tasks;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Modules;
using Microsoft.Extensions.Logging;

namespace Masya.TelegramBot.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public sealed class CommandsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICommandService _commands;
        private readonly ILogger<CommandsController> _logger;

        public CommandsController(
            ApplicationDbContext dbContext,
            ICommandService commands,
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
            var commands = await _dbContext.Commands
                .Include(c => c.Aliases)
                .ToListAsync();
            return Ok(commands.Where(c => c.ParentId == null));
        }

        [HttpPost("reload")]
        public async Task<IActionResult> ReloadCommandsAsync()
        {
            _logger.LogInformation("Reloading commands and aliases...");
            await _commands.LoadCommandsAsync(typeof(BasicModule).Assembly);
            _logger.LogInformation("Reloaded all commands and aliases.");
            return Ok();
        }

        [HttpPut("save")]
        public async Task<IActionResult> SaveCommandsAsync(IEnumerable<Command> commands)
        {
            _logger.LogInformation("Received a request to update commands and aliases.");
            _dbContext.Commands.UpdateRange(commands);
            await _dbContext.SaveChangesAsync();
            await _commands.LoadCommandsAsync(typeof(BasicModule).Assembly);
            _logger.LogInformation("Updated commands and reloaded the command service.");
            return Ok();
        }

        // [HttpPost("add")]
        // public async Task<IActionResult> AddCommandAsync(CommandDto dto)
        // {
        //     Command newCommand = new Command
        //     {
        //         Name = dto.Name,
        //         Permission = dto.Permission,
        //         IsEnabled = dto.IsEnabled,
        //         DisplayInMenu = dto.DisplayInMenu
        //     };

        //     foreach (var alias in dto.Aliases)
        //     {
        //         newCommand.Aliases.Add(
        //             new Command()
        //             {
        //                 Name = dto.Name,
        //                 Permission = dto.Permission,
        //                 IsEnabled = dto.IsEnabled,
        //                 DisplayInMenu = dto.DisplayInMenu
        //             }
        //         );
        //     }

        //     _dbContext.Commands.Add(newCommand);
        //     await _dbContext.SaveChangesAsync();

        //     return Created("api/controller/add", dto);
        // }

        // [HttpDelete("delete")]
        // public async Task<IActionResult> DeleteCommandAsync(int id)
        // {
        //     var command = await _dbContext.Commands.FirstOrDefaultAsync(c => c.Id == id);
        //     if (command == null)
        //     {
        //         return BadRequest(new MessageResponseDto("Command not found."));
        //     }

        //     _dbContext.Commands.Remove(command);
        //     await _dbContext.SaveChangesAsync();
        //     return Ok();
        // }
    }
}