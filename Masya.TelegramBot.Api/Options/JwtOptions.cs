using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Masya.TelegramBot.Api.Options
{
    public sealed class JwtOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int Expires { get; set; }
        public string Secret { get; set; }
        public SymmetricSecurityKey SecurityKey =>
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Secret));
    }
}