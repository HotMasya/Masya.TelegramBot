using System.Collections.Generic;
using System.Security.Claims;
using Masya.TelegramBot.Api.Options;
using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.Api.Services
{
    public interface IJwtService
    {
        JwtOptions Options { get; }
        string GenerateAccessToken(User user);
        string GenerateRefreshToken(User user);
        IEnumerable<Claim> GetClaims(string token);
    }
}