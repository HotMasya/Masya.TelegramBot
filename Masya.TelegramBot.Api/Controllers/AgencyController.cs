using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Masya.TelegramBot.Api.Dtos;
using Masya.TelegramBot.Api.Services.Abstractions;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Masya.TelegramBot.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public sealed class AgencyController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _dbContext;
        private readonly IDatabaseLogsService _logs;
        private readonly ILogger<AgencyController> _logger;

        public AgencyController(
            ApplicationDbContext dbContext,
            IMapper mapper,
            IDatabaseLogsService logs,
            ILogger<AgencyController> logger
        )
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _logs = logs;
            _logger = logger;
        }

        private async Task<Agency> GetUserAgencyAsync()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (long.TryParse(userIdClaim.Value, out long telegramId))
            {
                return await _dbContext.Agencies
                    .Include(a => a.Users)
                    .FirstOrDefaultAsync(
                        a => a.Users.Any(u => u.TelegramAccountId == telegramId)
                    );
            }

            return null;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllAgenciesAsync()
        {
            if (!User.HasPermission(Permission.SuperAdmin))
            {
                return Forbid();
            }

            var agencies = await _dbContext.Agencies.ToArrayAsync();
            var agenciesDtos = _mapper.Map<AgencyDto[]>(agencies);
            return Ok(agenciesDtos);
        }

        [HttpPost("all/save")]
        public async Task<IActionResult> SaveAllAgenciesAsync(AgencyDto[] dto)
        {
            if (!User.HasPermission(Permission.SuperAdmin))
            {
                return Forbid();
            }

            var agencies = await _dbContext.Agencies.ToListAsync();

            var agenciesIdsToDelete = agencies
                    .Select(r => r.Id)
                    .Except(
                        dto
                        .Where(a => a.Id.HasValue)
                        .Select(a => a.Id.Value)
                    );

            var agenciesToDelete = agencies.Where(
                a => agenciesIdsToDelete.FirstOrDefault(id => a.Id == id) != default
            );

            _dbContext.Agencies.RemoveRange(agenciesToDelete);

            foreach (var agencyDto in dto)
            {
                if (!agencyDto.Id.HasValue)
                {
                    _dbContext.Agencies.Add(_mapper.Map<Agency>(agencyDto));
                    continue;
                }

                var agency = agencies.FirstOrDefault(a => a.Id == agencyDto.Id.Value);

                if (agency is null) continue;

                _mapper.Map(agency, agencyDto);
            }

            await _dbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("import/logs")]
        public async Task<IActionResult> GetImportLogsAsync([FromQuery] int agencyId)
        {
            return Ok(await _logs.GetAgencyLogsForLastDay(agencyId));
        }

        [HttpGet]
        public async Task<IActionResult> GetAgencyAsync()
        {
            var userAgency = await GetUserAgencyAsync();
            if (userAgency == null)
            {
                return BadRequest(new MessageResponseDto("The user is not an admin of the agency."));
            }

            var dto = _mapper.Map<AgencyDto>(userAgency);

            return Ok(dto);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAgencyAsync([FromBody] AgencyDto dto)
        {
            if (!User.HasPermission(Permission.SuperAdmin)) return Forbid();

            var result = _mapper.Map<Agency>(dto);

            _dbContext.Agencies.Add(result);
            await _dbContext.SaveChangesAsync();

            return Created("/api/agency/create", result);
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveAgencyAsync([FromBody] AgencyDto dto)
        {
            var userAgency = await GetUserAgencyAsync();
            if (userAgency == null)
            {
                return BadRequest(new MessageResponseDto("The user is not an admin of the agency."));
            }
            _mapper.Map(dto, userAgency);

            var users = userAgency.Users;
            var usersToDeleteIds = users.Select(u => u.Id).Except(dto.Agents.Select(d => d.Id));
            var usersToDelete = users.Where(u => usersToDeleteIds.FirstOrDefault(id => u.Id == id) != default(long));

            _dbContext.Users.RemoveRange(usersToDelete);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}