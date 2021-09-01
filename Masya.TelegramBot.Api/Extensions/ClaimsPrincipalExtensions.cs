using System.Linq;
using Masya.TelegramBot.DataAccess.Models;
using System.Security.Claims;
using System;

namespace Microsoft.AspNetCore.Mvc
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool HasPermission(this ClaimsPrincipal user, Permission permission)
        {
            var roleClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            if (roleClaim is null)
            {
                return false;
            }

            var claimPermission = Enum.Parse<Permission>(roleClaim.Value);

            return claimPermission >= permission;
        }
    }
}