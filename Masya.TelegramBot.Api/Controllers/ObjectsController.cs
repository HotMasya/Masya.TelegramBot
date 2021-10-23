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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class ObjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public ObjectsController(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetObjectsAsync()
        {
            if (!User.HasPermission(Permission.SuperAdmin))
            {
                return Forbid();
            }

            var objects = await _dbContext.RealtyObjects.ToArrayAsync();

            return Ok(
                _mapper.Map<RealtyObjectDto[]>(objects)
            );
        }

        [HttpPost]
        public async Task<IActionResult> SaveObjectsAsync(RealtyObjectDto[] dto)
        {
            if (!User.HasPermission(Permission.SuperAdmin))
            {
                return Forbid();
            }

            var objects = await _dbContext.RealtyObjects.ToListAsync();

            var objectsIdsToDelete = objects
                    .Select(o => o.Id)
                    .Except(
                        dto
                        .Where(o => o.Id.HasValue)
                        .Select(o => o.Id.Value)
                    );

            var objectsToDelete = objects.Where(
                o => objectsIdsToDelete.FirstOrDefault(id => o.Id == id) != default
            );

            _dbContext.RealtyObjects.RemoveRange(objectsToDelete);

            foreach (var objectDto in dto)
            {
                if (!objectDto.Id.HasValue)
                {
                    _dbContext.RealtyObjects.Add(_mapper.Map<RealtyObject>(objectDto));
                    continue;
                }

                var obj = objects.FirstOrDefault(o => o.Id == objectDto.Id.Value);

                if (obj is null) continue;

                _mapper.Map(objectDto, obj);
            }

            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}