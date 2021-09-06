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

        private async Task<Agency> GetUsetAgencyAsync(int? agencyId = null)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var user = await _dbContext.Users.Include(u => u.Agency).FirstOrDefaultAsync(u => u.TelegramAccountId == long.Parse(userIdClaim.Value));
            if (user == null || !agencyId.HasValue || agencyId.Value != user.AgencyId)
            {
                return null;
            }

            return user.Agency;
        }

        [HttpGet]
        public async Task<IActionResult> GetAgencyAsync()
        {
            var userAgency = await GetUsetAgencyAsync();
            if (userAgency == null)
            {
                return BadRequest(new MessageResponseDto("The user is not an admin of the agency."));
            }

            return Ok(userAgency);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAgencyAsync(AgencyDto dto)
        {
            if (!User.HasPermission(Permission.SuperAdmin)) return Forbid();

            var result = _mapper.Map<Agency>(dto);

            _dbContext.Agencies.Add(result);
            await _dbContext.SaveChangesAsync();

            return Created("/api/agency/create", result);
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveAgencyAsync(Agency agency)
        {
            var userAgency = await GetUsetAgencyAsync(agency.Id);
            if (userAgency == null)
            {
                return BadRequest(new MessageResponseDto("The user is not an admin of the agency."));
            }

            _mapper.Map(agency, userAgency);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}