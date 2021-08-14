using System.Threading.Tasks;
using Masya.TelegramBot.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Masya.TelegramBot.Api.Controllers
{
    [Authorize]
    public sealed class CommandController : ControllerBase
    {

        private readonly ApplicationDbContext _dbContext;

        public CommandController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var commands = await _dbContext.Commands.Include(c => c.Aliases).ToListAsync();
            return Ok(commands);
        }
    }
}