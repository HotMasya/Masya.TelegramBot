using System.Threading.Tasks;
using Masya.TelegramBot.Api.Dtos;
using Masya.TelegramBot.Commands.Data;
using Masya.TelegramBot.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Masya.TelegramBot.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public sealed class CommandController : ControllerBase
    {

        private readonly ApplicationDbContext _dbContext;

        public CommandController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var commands = await _dbContext.Commands.Include(c => c.Aliases).ToListAsync();
            return Ok(commands);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddCommandAsync(CommandDto dto)
        {
            Command newCommand = new Command
            {
                Name = dto.Name,
                Permission = dto.Permission,
                IsEnabled = dto.IsEnabled,
                DisplayInMenu = dto.DisplayInMenu
            };

            foreach (var alias in dto.Aliases)
            {
                newCommand.Aliases.Add(
                    new Command()
                    {
                        Name = dto.Name,
                        Permission = dto.Permission,
                        IsEnabled = dto.IsEnabled,
                        DisplayInMenu = dto.DisplayInMenu
                    }
                );
            }

            _dbContext.Commands.Add(newCommand);
            await _dbContext.SaveChangesAsync();

            return Created("api/controller/add", dto);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteCommandAsync(int id)
        {
            var command = await _dbContext.Commands.FirstOrDefaultAsync(c => c.Id == id);
            if (command == null)
            {
                return BadRequest(new MessageResponseDto("Command not found."));
            }

            _dbContext.Commands.Remove(command);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}