using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Commands.Options;
using Masya.TelegramBot.Commands.Services;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Models;
using Masya.TelegramBot.DataAccess.Types;
using Masya.TelegramBot.DatabaseExtensions.Abstractions;
using Masya.TelegramBot.DatabaseExtensions.Metadata;
using Masya.TelegramBot.DatabaseExtensions.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Masya.TelegramBot.DatabaseExtensions
{
    public sealed class DatabaseCommandService : DefaultCommandService<DatabaseCommandInfo, DatabaseAliasInfo>
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

        public override bool CheckCommandCondition(DatabaseCommandInfo commandInfo, Message message)
        {

            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = dbContext.Users.FirstOrDefault(u => u.TelegramAccountId == message.From.Id);

            return (
                base.CheckCommandCondition(commandInfo, message) &&
                commandInfo.Permission == Permission.Guest || (
                    user != null &&
                    user.Permission >= commandInfo.Permission
                )
            );
        }

        private async Task SendRoleQuestionAsync(long chatId)
        {
            await BotService.Client.SendTextMessageAsync(
                chatId: chatId,
                text: "Who are you?",
                replyMarkup: KeyboardGenerator.Roles()
            );
        }

        private Task HandleContactAsync(Message message)
        {
            var contact = message.Contact;
            if (!contact.UserId.HasValue || contact.UserId.Value != message.From.Id) return Task.CompletedTask;

            var dbUser = new DataAccess.Models.User()
            {
                TelegramAccountId = contact.UserId.Value,
                TelegramFirstName = contact.FirstName,
                TelegramLastName = contact.LastName,
                TelegramLogin = message.From.Username,
                TelegramPhoneNumber = contact.PhoneNumber
            };

            var collector = BotService
                .CreateMessageCollector(message.Chat, TimeSpan.FromSeconds(Options.StepCommandTimeout))
                .Collect(m => m.Contact)
                .Collect(m => m.Text);

            collector.OnStart += (sender, args) =>
            {
                SendRoleQuestionAsync(message.Chat.Id).Wait();
            };

            collector.OnMessageReceived += (sender, args) =>
            {
                if (dbUser.Permission == Permission.Agent)
                {
                    var key = args.Message.Text;
                    using var scope = services.CreateScope();
                    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var agency = ctx.Agencies.FirstOrDefault(a => a.RegistrationKey.Equals(key));

                    if (agency is not null)
                    {
                        dbUser.AgencyId = agency.Id;
                        dbUser.TelegramAvatar = BotService.Client
                            .DownloadAvatarAsync(contact.UserId.Value).GetAwaiter().GetResult();
                        BotService.Client.SendTextMessageAsync(
                            chatId: args.Message.Chat.Id,
                            text: string.Format("You're now the agent of the agency: *{0}*.", agency.Name),
                            parseMode: ParseMode.Markdown
                        ).Wait();
                        collector.Finish();
                        return;
                    }

                    dbUser.Permission = Permission.Guest;
                    BotService.Client.SendTextMessageAsync(
                        chatId: args.Message.Chat.Id,
                        text: "Invalid agency registration key."
                    ).Wait();
                    collector.Finish();
                    return;
                }

                string formattedText = args.Message.Text?.Trim()?.ToLower();
                switch (formattedText)
                {
                    case UserRoles.Customer:
                        dbUser.Permission = Permission.User;
                        dbUser.TelegramAvatar = BotService.Client
                            .DownloadAvatarAsync(contact.UserId.Value).GetAwaiter().GetResult();
                        BotService.Client.SendTextMessageAsync(
                            chatId: args.Message.Chat.Id,
                            text: "You are now registered as a customer."
                        ).Wait();
                        break;

                    case UserRoles.Agent:
                        dbUser.Permission = Permission.Agent;
                        BotService.Client.SendTextMessageAsync(
                            chatId: args.Message.Chat.Id,
                            text: "Enter agency registration key, please."
                        ).Wait();
                        return;

                    default:
                        SendRoleQuestionAsync(args.Message.Chat.Id).Wait();
                        return;
                }
                collector.Finish();
            };

            collector.OnFinish += (sender, args) =>
            {
                if (dbUser.Permission == Permission.Guest)
                {
                    BotService.Client.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Signing up failed. Please, try again.",
                        replyMarkup: new ReplyKeyboardRemove()
                    ).Wait();
                    return;
                }

                using var scope = services.CreateScope();
                var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var keyboards = scope.ServiceProvider.GetRequiredService<IKeyboardGenerator>();
                ctx.Users.Add(dbUser);
                ctx.SaveChanges();
                BotService.Client.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: MessageGenerator.GenerateMenuMessage(dbUser),
                    parseMode: ParseMode.Markdown,
                    replyMarkup: keyboards.Menu(dbUser.Permission)
                    ).Wait();
            };

            collector.OnMessageTimeout += (sender, args) =>
            {
                using var scope = services.CreateScope();
                var keyboards = scope.ServiceProvider.GetRequiredService<IKeyboardGenerator>();
                BotService.Client.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "The time is out, please, try again.",
                        replyMarkup: keyboards.Menu(dbUser.Permission)
                    ).Wait();
            };

            collector.Start();
            return Task.CompletedTask;
        }

        public override async Task ExecuteCommandAsync(Message message)
        {
            if (message.Contact != null)
            {
                await HandleContactAsync(message);
            }
            await base.ExecuteCommandAsync(message);
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
