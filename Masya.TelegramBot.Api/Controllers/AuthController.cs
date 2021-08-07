using System;
using System.Linq;
using System.Threading.Tasks;
using Masya.TelegramBot.Api.Dtos;
using Masya.TelegramBot.Api.Services;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Masya.TelegramBot.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public sealed class AuthController : ControllerBase
    {

        private readonly ApplicationDbContext _dbContext;
        private readonly IBotService _botService;
        private readonly IDistributedCache _cache;
        private readonly IJwtService _jwtService;
        private const string AuthCodePrefix = "AuthCode_";

        public AuthController(
            ApplicationDbContext dbContext,
            IBotService botService,
            IDistributedCache cache,
            IJwtService jwtService)
        {
            _dbContext = dbContext;
            _botService = botService;
            _cache = cache;
            _jwtService = jwtService;
        }

        [HttpPost("phone")]
        public async Task<IActionResult> AuthPhoneAsync(PhoneDto dto)
        {
            var user = _dbContext.Users
                .FirstOrDefault(u => u.TelegramPhoneNumber.Equals(dto.PhoneNumber));

            if (user == null)
            {
                return BadRequest(new ResponseDto<object>("User not found."));
            }

            var rng = new Random();
            int code1 = rng.Next(1, 1000);
            int code2 = rng.Next(1, 1000);
            int fullCode = code1 * 1000 + code2;
            string message = string.Format("Your code: {0} {1}.", code1, code2);
            string recordId = AuthCodePrefix + fullCode;
            await _cache.SetRecordAsync(recordId, user.TelegramAccountId, TimeSpan.FromSeconds(60));
            await _botService.Client.SendTextMessageAsync(user.TelegramAccountId, message);
            return Ok();
        }

        [HttpPost("code")]
        public async Task<IActionResult> AuthCodeAsync(CodeDto dto)
        {
            string recordId = AuthCodePrefix + dto.Code;
            var userTelegramId = await _cache.GetRecordAsync<int>(recordId);
            if (userTelegramId == default(int))
            {
                return BadRequest(new ResponseDto<object>("Code is invalid."));
            }

            var user = _dbContext.Users.FirstOrDefault(u => u.TelegramAccountId == userTelegramId);
            if (user == null)
            {
                return BadRequest(new ResponseDto<object>("User not found."));
            }

            string token = _jwtService.GenerateToken(user);
            return Ok(new TokenDto(token));
        }
    }
}