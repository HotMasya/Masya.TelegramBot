using System.Collections.Generic;
using System.Reflection;
using Masya.TelegramBot.Commands.Metadata;
using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.DatabaseExtensions.Metadata
{
    public sealed class DatabaseCommandInfo : CommandInfo<DatabaseAliasInfo>
    {
        public Permission Permission { get; set; }

        public DatabaseCommandInfo() : base()
        {
            Permission = Permission.Guest;
        }

        public DatabaseCommandInfo(
            string name,
            string description,
            Permission permission,
            MethodInfo methodInfo,
            IList<DatabaseAliasInfo> aliases = null
        ) : base(name, description, methodInfo, aliases)
        {
            Permission = permission;
        }
    }
}