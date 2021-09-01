using System.Linq;
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

        [HttpGet("/")]
        [Authorize]
        public async Task<IActionResult> LoadUsersAsync()
        {
            if (!User.HasPermission(Permission.SuperAdmin)) return Forbid();

            var users = await _dbContext.Users.Include(u => u.Agency).ToListAsync();
            var dtos = _mapper.Map<UserDto[]>(users);

            return Ok(dtos);
        }

        [HttpPost("/save")]
        [Authorize]
        public async Task<IActionResult> SaveUsersAsync(UserDto[] dtos)
        {
            if (!User.HasPermission(Permission.SuperAdmin)) return Forbid();
            var users = await _dbContext.Users.ToListAsync();

            foreach (var dto in dtos)
            {
                var user = users.FirstOrDefault(u => u.Id == dto.Id);
                if (user is null) continue;
                _mapper.Map(dto, user);
            }

            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}