using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.Commands.Metadata
{
    public sealed class AliasInfo
    {
        public string Name { get; set; }
        public bool? IsEnabled { get; set; }
        public Permission Permission { get; set; }
    }
}