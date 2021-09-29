using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Masya.TelegramBot.Api.Dtos;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Masya.TelegramBot.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public sealed class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public UsersController(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [HttpGet("me")]
        public IActionResult Me()
        {
            var idClaim = User.Claims.First(u => u.Type == ClaimTypes.NameIdentifier);
            if (long.TryParse(idClaim.Value, out long telegramUserId))
            {
                var user = _dbContext.Users
                    .Include(u => u.Agency)
                    .FirstOrDefault(u => u.TelegramAccountId == telegramUserId);

                if (user is null)
                {
                    return BadRequest(new ResponseDto<object>("Invalid access token."));
                }

                var dto = _mapper.Map<UserDto>(user);
                return Ok(dto);
            }

            return BadRequest(new ResponseDto<object>("Invalid access token."));
        }

        [HttpGet]
        public async Task<IActionResult> LoadUsersAsync()
        {
            if (!User.HasPermission(Permission.SuperAdmin)) return Forbid();

            var users = await _dbContext.Users.Include(u => u.Agency).ToListAsync();
            var dtos = _mapper.Map<UserDto[]>(users);

            return Ok(dtos);
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveUsersAsync([FromBody] UserDto[] dtos)
        {
            if (!User.HasPermission(Permission.SuperAdmin)) return Forbid();

            var users = await _dbContext.Users.ToListAsync();
            var usersToDeleteIds = users.Select(u => u.Id).Except(dtos.Select(d => d.Id));
            var usersToDelete = users.Where(u => usersToDeleteIds.FirstOrDefault(id => u.Id == id) != default);

            foreach (var dto in dtos)
            {
                var user = users.FirstOrDefault(u => u.Id == dto.Id);
                if (user is null) continue;
                _mapper.Map(dto, user);
            }

            _dbContext.Users.RemoveRange(usersToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}