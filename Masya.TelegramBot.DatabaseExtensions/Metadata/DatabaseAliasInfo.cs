using Masya.TelegramBot.Commands.Metadata;
using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.DatabaseExtensions.Metadata
{
    public sealed class DatabaseAliasInfo : AliasInfo
    {
        public Permission Permission { get; set; }
    }
}