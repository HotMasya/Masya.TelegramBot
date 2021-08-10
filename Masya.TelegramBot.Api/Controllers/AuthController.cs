using System;
using System.Linq;
using System.Threading.Tasks;
using Masya.TelegramBot.Api.Dtos;
using Masya.TelegramBot.Api.Services;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Telegram.Bot.Types.Enums;

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
            string messageWithCode = string.Format("Your code: <b>{0} {1}</b>.", code1, code2);
            string recordId = AuthCodePrefix + fullCode;
            await _cache.SetRecordAsync(recordId, user.TelegramPhoneNumber, TimeSpan.FromSeconds(60));
            await _botService.Client.SendTextMessageAsync(user.TelegramAccountId, messageWithCode, ParseMode.Html);
            return Ok();
        }

        [HttpPost("code")]
        public async Task<IActionResult> AuthCodeAsync(CodeDto dto)
        {
            string recordId = AuthCodePrefix + dto.Code;
            var userPhoneNumber = await _cache.GetRecordAsync<string>(recordId);
            if (string.IsNullOrEmpty(userPhoneNumber))
            {
                return BadRequest(new ResponseDto<object>("Code is invalid."));
            }

            var user = _dbContext.Users.FirstOrDefault(u => u.TelegramPhoneNumber == userPhoneNumber);
            if (user == null)
            {
                return BadRequest(new ResponseDto<object>("Code is invalid."));
            }

            string token = _jwtService.GenerateToken(user);
            return Ok(new TokenDto(token));
        }
    }
}