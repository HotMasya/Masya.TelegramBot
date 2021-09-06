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
    [Route("/api/[controller]")]
    public sealed class AgencyController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _dbContext;

        public AgencyController(ApplicationDbContext dbContext, IMapper mapper)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAgency(AgencyDto dto)
        {
            if (!User.HasPermission(Permission.SuperAdmin)) return Forbid();

            var result = _mapper.Map<Agency>(dto);

            _dbContext.Agencies.Add(result);
            await _dbContext.SaveChangesAsync();

            return Created("/api/agency/create", result);
        }

        [HttpGet]
        public async Task<IActionResult> SaveAgency(Agency agency)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var user = await _dbContext.Users.Include(u => u.Agency).FirstOrDefaultAsync(u => u.TelegramAccountId == long.Parse(userIdClaim.Value));
            if (user == null || agency.Id != user.AgencyId)
            {
                return BadRequest(new MessageResponseDto("The user is not an admin of the agency."));
            }

            _mapper.Map(agency, user.Agency);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}