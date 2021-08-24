using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Commands.Metadata;
using Masya.TelegramBot.Commands.Options;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands.Services
{
    public class DatabaseCommandService : DefaultCommandService
    {
        public DatabaseCommandService(
            IOptionsMonitor<CommandServiceOptions> options,
            IBotService botService,
            IServiceProvider services,
            ILogger<DefaultCommandService> logger
            )
            : base(options, botService, services, logger) { }

        public override async Task LoadCommandsAsync(Assembly assembly)
        {
            await base.LoadCommandsAsync(assembly);
            await MapCommandsAsync();
        }

        public override bool CheckCommandCondition(CommandInfo commandInfo, Message message)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = dbContext.Users.FirstOrDefault(u => u.TelegramAccountId == message.From.Id);

            _logger.LogInformation("\nUser:\t{0}\nUser.Permission:\t{1}\nCommandInfo.Permission:\t{2}",
                user is not null,
                user.Permission.HasValue,
                commandInfo.Permission.HasValue
            );

            return (
                base.CheckCommandCondition(commandInfo, message) &&
                user is not null &&
                user.Permission.HasValue &&
                commandInfo.Permission.HasValue &&
                (user.Permission.Value == Permission.All || user.Permission.Value >= commandInfo.Permission.Value)
            );
        }

        private async Task MapCommandsAsync()
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dbCommands = await dbContext.Commands.ToListAsync();
            foreach (var ci in commands)
            {
                var command = dbCommands.FirstOrDefault(
                    c => c.Name.ToLower().Equals(ci.Name?.ToLower())
                );
                if (command != null)
                {
                    ci.IsEnabled = command.IsEnabled;
                    ci.Permission = command.Permission;
                    foreach (var al in command.Aliases)
                    {
                        ci.Aliases.Add(
                            new AliasInfo
                            {
                                Name = al.Name,
                                CommandInfo = ci,
                            }
                        );
                    }
                }
            }
        }
    }
}
