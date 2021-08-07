using System.Collections.Generic;
using System.Security.Claims;
using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.Api.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        IEnumerable<Claim> GetClaims(string token);
    }
}