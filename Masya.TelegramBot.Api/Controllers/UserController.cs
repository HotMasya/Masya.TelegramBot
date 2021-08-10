using System.Linq;
using System.Security.Claims;
using Masya.TelegramBot.Api.Dtos;
using Masya.TelegramBot.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Masya.TelegramBot.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public sealed class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public UserController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("me")]
        public IActionResult Me()
        {
            var idClaim = User.Claims.First(u => u.Type == ClaimTypes.NameIdentifier);
            if (long.TryParse(idClaim.Value, out long telegramUserId))
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.TelegramAccountId == telegramUserId);
                if (user is null)
                {
                    return BadRequest(new ResponseDto<object>("Invalid access token."));
                }
                var dto = new UserDto(user);
                return Ok(dto);
            }

            return BadRequest(new ResponseDto<object>("Invalid access token."));
        }
    }
}