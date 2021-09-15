using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Masya.TelegramBot.Api.Options;
using Masya.TelegramBot.DataAccess.Models;
using Masya.TelegramBot.Api.Services.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Masya.TelegramBot.Api.Services
{
  public sealed class JwtService : IJwtService
  {
    public JwtOptions Options { get; }

    public JwtService(IOptions<JwtOptions> options)
    {
      Options = options.Value;
    }

    private string GenerateToken(Claim[] claims, DateTime expires)
    {
      var tokenHandler = new JwtSecurityTokenHandler();
      var tokenDescriptor = new SecurityTokenDescriptor()
      {
        Subject = new ClaimsIdentity(claims),
        Expires = expires,
        Issuer = Options.Issuer,
        Audience = Options.Audience,
        SigningCredentials = new SigningCredentials(Options.SecurityKey, SecurityAlgorithms.HmacSha256Signature),
      };

      var token = tokenHandler.CreateToken(tokenDescriptor);
      return tokenHandler.WriteToken(token);
    }

    public string GenerateAccessToken(User user)
    {
      Claim[] claims = new Claim[] {
            new Claim(ClaimTypes.NameIdentifier, user.TelegramAccountId.ToString()),
            new Claim(ClaimTypes.Role, user.Permission.ToString(), ClaimValueTypes.UInteger32)
        };

      return GenerateToken(claims, DateTime.Now.AddMinutes(Options.ExpiresInMinutes));
    }

    public string GenerateRefreshToken(User user)
    {
      Claim[] claims = new Claim[] {
                new Claim(ClaimTypes.Name, user.TelegramLogin)
            };

      return GenerateToken(claims, DateTime.Now.AddDays(Options.RefreshExpiresInDays));
    }

    public ClaimsPrincipal Validate(string token)
    {
      var parameters = new TokenValidationParameters()
      {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = Options.Issuer,
        ValidAudience = Options.Audience,
        IssuerSigningKey = Options.SecurityKey
      };

      var tokenHandler = new JwtSecurityTokenHandler();

      try
      {
        var principal = tokenHandler
            .ValidateToken(token, parameters, out SecurityToken resultToken);
        return principal;
      }
      catch
      {
        return null;
      }
    }
  }
}