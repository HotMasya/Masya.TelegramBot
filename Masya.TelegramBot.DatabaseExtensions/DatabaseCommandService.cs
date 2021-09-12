using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Commands.Options;
using Masya.TelegramBot.Commands.Services;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Models;
using Masya.TelegramBot.DatabaseExtensions.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Masya.TelegramBot.DatabaseExtensions
{
    public class DatabaseCommandService : DefaultCommandService<DatabaseCommandInfo, DatabaseAliasInfo>
    {
        public DatabaseCommandService(
            IOptionsMonitor<CommandServiceOptions> options,
            IBotService<DatabaseCommandInfo, DatabaseAliasInfo> botService,
            IServiceProvider services,
            ILogger<DefaultCommandService<DatabaseCommandInfo, DatabaseAliasInfo>> logger
            )
            : base(options, botService, services, logger) { }

        public override async Task LoadCommandsAsync(Assembly assembly)
        {
            await base.LoadCommandsAsync(assembly);
            await MapCommandsAsync();
        }

        private void UpdateButtons(List<List<KeyboardButton>> buttons, ref int currentRowIndex)
        {
            if (buttons.Count == 0)
            {
                buttons.Add(new List<KeyboardButton>());
            }
            else if (buttons[currentRowIndex].Count == Options.MaxMenuColumns)
            {
                currentRowIndex++;
                buttons.Add(new List<KeyboardButton>());
            }
        }

        public IReplyMarkup GetMenuKeyboard(Permission userPermission)
        {
            var buttons = new List<List<KeyboardButton>>();
            int currentRowIndex = 0;
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var commands = dbContext.Commands
                .AsQueryable()
                .Where(c => c.ParentCommand == null)
                .Include(c => c.Aliases)
                .ToList();

            if (commands.Count == 0)
            {
                return new ReplyKeyboardMarkup();
            }

            foreach (var c in commands)
            {
                if (c.DisplayInMenu && userPermission >= c.Permission)
                {
                    UpdateButtons(buttons, ref currentRowIndex);
                    buttons[currentRowIndex].Add(new KeyboardButton(c.Name));
                }

                if (c.Aliases is not null && c.Aliases.Count > 0)
                {
                    foreach (var a in c.Aliases)
                    {
                        if (a.DisplayInMenu && userPermission >= a.Permission)
                        {
                            UpdateButtons(buttons, ref currentRowIndex);
                            buttons[currentRowIndex].Add(new KeyboardButton(a.Name));
                        }
                    }
                }
            }
            return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
        }

        public override bool CheckCommandCondition(DatabaseCommandInfo commandInfo, Message message)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = dbContext.Users.FirstOrDefault(u => u.TelegramAccountId == message.From.Id);

            logger.LogInformation("CheckCommandCondition command permission: " + commandInfo.Permission.ToString());
            logger.LogInformation("CheckCommandCondition user permission: " + user.Permission.ToString());

            return (
                base.CheckCommandCondition(commandInfo, message) &&
                commandInfo.Permission == Permission.Guest || (
                    user is not null &&
                    user.Permission >= commandInfo.Permission
                )
            );
        }

        protected override DatabaseCommandInfo GetCommand(string name, Message message)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = dbContext.Users.FirstOrDefault(u => u.TelegramAccountId == message.From.Id);
            return commands?.FirstOrDefault(cm => DatabaseCommandFilter(cm, name?.ToLower(), user));
        }

        private static bool DatabaseCommandFilter(DatabaseCommandInfo commandInfo, string commandName, DataAccess.Models.User user)
        {
            return commandInfo.Name.ToLower().Equals(commandName)
                || commandInfo.Aliases.Any(
                        a => a.Name.ToLower().Equals(commandName)
                        && a.IsEnabled
                        && (
                            a.Permission == Permission.Guest || (
                                user is not null &&
                                user.Permission >= a.Permission
                            )
                        )
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
                            new DatabaseAliasInfo
                            {
                                Name = al.Name,
                                IsEnabled = al.IsEnabled,
                                Permission = al.Permission,
                            }
                        );
                    }
                }
            }
        }
    }
}
