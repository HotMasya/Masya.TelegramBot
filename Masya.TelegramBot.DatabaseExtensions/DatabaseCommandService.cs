using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Commands.Metadata;
using Masya.TelegramBot.Commands.Options;
using Masya.TelegramBot.Commands.Services;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Models;
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

        public override IReplyMarkup GetMenuKeyboard()
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
                if (c.DisplayInMenu)
                {
                    UpdateButtons(buttons, ref currentRowIndex);
                    buttons[currentRowIndex].Add(new KeyboardButton(c.Name));
                }

                if (c.Aliases is not null && c.Aliases.Count > 0)
                {
                    foreach (var a in c.Aliases)
                    {
                        if (a.DisplayInMenu)
                        {
                            UpdateButtons(buttons, ref currentRowIndex);
                            buttons[currentRowIndex].Add(new KeyboardButton(a.Name));
                        }
                    }
                }
            }
            return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
        }

        public override bool CheckCommandCondition(CommandInfo commandInfo, Message message)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = dbContext.Users.FirstOrDefault(u => u.TelegramAccountId == message.From.Id);
            return (
                base.CheckCommandCondition(commandInfo, message) &&
                commandInfo.Permission == Permission.Guest || (
                    user is not null &&
                    user.Permission >= commandInfo.Permission
                )
            );
        }

        protected override CommandInfo GetCommand(string name, Message message)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = dbContext.Users.FirstOrDefault(u => u.TelegramAccountId == message.From.Id);
            return commands.FirstOrDefault(cm => DatabaseCommandFilter(cm, name, user));
        }

        protected bool DatabaseCommandFilter(CommandInfo commandInfo, string commandName, DataAccess.Models.User user)
        {
            _logger.LogInformation(
                "CommandInfo name: {0}\nCommand name: {1}",
                commandInfo.Name,
                commandName
            );

            return !string.IsNullOrEmpty(commandInfo.Name) &&
            (
                commandInfo.Name.Equals(commandName) ||
                commandInfo.Aliases.Any(
                    a => a.Name.Equals(commandName)
                    && a.IsEnabled
                    && (
                        a.Permission == Permission.Guest || (
                            user is not null &&
                            user.Permission >= a.Permission
                        )
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
                            new AliasInfo
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
