using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Masya.TelegramBot.Api.Dtos;
using Masya.TelegramBot.Api.Services;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Telegram.Bot.Types.Enums;
using Masya.TelegramBot.Api.Options;
using Microsoft.Extensions.Options;
using Masya.TelegramBot.DataAccess.Models;

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
        private readonly CacheOptions _cacheOptions;

        private const string AuthCodePrefix = "AuthCode_";

        public AuthController(
            ApplicationDbContext dbContext,
            IBotService botService,
            IDistributedCache cache,
            IJwtService jwtService,
            IOptionsMonitor<CacheOptions> cacheOptions)
        {
            _dbContext = dbContext;
            _botService = botService;
            _cache = cache;
            _jwtService = jwtService;
            _cacheOptions = cacheOptions.CurrentValue;
        }

        [HttpPost("refresh")]
        public IActionResult RefreshToken([FromBody] string refreshToken)
        {
            var claims = _jwtService.GetClaims(refreshToken);
            var expires = claims.FirstOrDefault(c => c.Type == ClaimTypes.Expiration);
            var userName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;
            var expiresDateTime = DateTime.Parse(expires.Value);
            if (DateTime.Now > expiresDateTime)
            {
                return BadRequest(new MessageResponseDto("Refresh token is expired."));
            }

            if (userName is null)
            {
                return BadRequest(new MessageResponseDto("Invalid refresh token."));
            }

            var user = _dbContext.Users.FirstOrDefault(u => u.TelegramLogin.Equals(userName));

            if (user is null)
            {
                return BadRequest(new MessageResponseDto("Invalid refresh token."));
            }

            string newAccessToken = _jwtService.GenerateAccessToken(user);
            return Ok(new TokenDto(newAccessToken));
        }

        [HttpPost("phone")]
        public async Task<IActionResult> AuthPhoneAsync(PhoneDto dto)
        {
            var user = _dbContext.Users
                .FirstOrDefault(u => u.TelegramPhoneNumber.Equals(dto.PhoneNumber));

            if (user == null || user.Permission < Permission.Admin)
            {
                return BadRequest(new MessageResponseDto("Invalid phone number."));
            }

            var rng = new Random();
            int code1 = rng.Next(100, 1000);
            int code2 = rng.Next(100, 1000);
            int fullCode = code1 * 1000 + code2;
            string messageWithCode = string.Format(
                $"Your code: <b>{0} {1}</b>.\nIt will be valid only for {_cacheOptions.CodeDurationInSecs} seconds.",
                code1,
                code2
            );
            string recordId = AuthCodePrefix + fullCode;
            await _cache.SetRecordAsync(
                recordId: recordId,
                item: user.TelegramPhoneNumber,
                TimeSpan.FromSeconds(60)
            );
            await _botService.Client.SendTextMessageAsync(
                chatId: user.TelegramAccountId,
                text: messageWithCode,
                parseMode: ParseMode.Html
            );
            return Ok();
        }

        [HttpPost("code")]
        public async Task<IActionResult> AuthCodeAsync(CodeDto dto)
        {
            string recordId = AuthCodePrefix + dto.Code;
            var userPhoneNumber = await _cache.GetRecordAsync<string>(recordId);
            if (string.IsNullOrEmpty(userPhoneNumber))
            {
                return BadRequest(new MessageResponseDto("Code is invalid."));
            }

            var user = _dbContext.Users.FirstOrDefault(u => u.TelegramPhoneNumber == userPhoneNumber);
            if (user == null)
            {
                return BadRequest(new MessageResponseDto("Code is invalid."));
            }

            await _cache.RemoveAsync(recordId);
            string accessToken = _jwtService.GenerateAccessToken(user);
            string refreshToken = _jwtService.GenerateRefreshToken(user);
            return Ok(new TokenDto(accessToken, refreshToken));
        }
    }
}