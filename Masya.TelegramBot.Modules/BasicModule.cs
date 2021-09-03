using System;
using System.Linq;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands;
using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.Commands.Options;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Masya.TelegramBot.DataAccess.Models;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

namespace Masya.TelegramBot.Modules
{
    public sealed class BasicModule : Module
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly CommandServiceOptions _options;
        private readonly IServiceProvider _services;

        public BasicModule(
            ApplicationDbContext context,
            IServiceProvider services,
            IOptions<CommandServiceOptions> options
            )
        {
            _dbContext = context;
            _options = options.Value;
            _services = services;
        }

        private static string GenerateMenuMessage(Message message, ApplicationDbContext dbContext)
        {
            var user = dbContext.Users.First(u => u.TelegramAccountId == message.From.Id);
            var fullName = user.TelegramFirstName + (
                string.IsNullOrEmpty(user.TelegramLastName)
                    ? ""
                    : " " + user.TelegramLastName
            );

            return string.Format(
                "Welcome back, <b>{0}</b>!\nYour status: <b>{1}</b>.\nYour are in main menu now.",
                fullName,
                user.Permission.ToString()
            );
        }

        [Command("/start")]
        public async Task StartCommandAsync()
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.TelegramAccountId == Context.User.Id);
            if (user is null)
            {
                await ReplyAsync(
                    "First, you have to sign up.",
                    replyMarkup: new ReplyKeyboardMarkup(KeyboardButton.WithRequestContact("Send information about me."))
                );
                return;
            }

            await ReplyAsync(GenerateMenuMessage(Context.Message, _dbContext), replyMarkup: Context.CommandService.GetMenuKeyboard(user.Permission));
        }

        [RegisterUser]
        public Task RegisterUserAsync(Contact contact)
        {
            var user = Context.Message.From;

            if (contact.UserId != user.Id) return Task.CompletedTask;

            var dbUser = new DataAccess.Models.User()
            {
                TelegramAccountId = contact.UserId,
                TelegramFirstName = contact.FirstName,
                TelegramLastName = contact.LastName,
                TelegramLogin = user.Username,
                TelegramPhoneNumber = contact.PhoneNumber
            };

            var collector = Context.BotService
                .CreateMessageCollector(Context.Message.Chat, TimeSpan.FromSeconds(_options.StepCommandTimeout))
                .Collect(m => m.Contact)
                .Collect(m => m.Text);

            var scope = _services.CreateScope();
            collector.OnStart += (sender, args) =>
            {
                SendRoleQuestionAsync(Context.Message.Chat.Id).Wait();
            };

            collector.OnMessageReceived += (sender, args) =>
            {
                if (dbUser.Permission == Permission.Agent)
                {
                    var key = args.Message.Text;
                    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var agency = ctx.Agencies.FirstOrDefault(a => a.RegistrationKey.Equals(key));

                    if (agency is not null)
                    {
                        dbUser.AgencyId = agency.Id;
                        Context.BotService.Client.SendTextMessageAsync(
                            chatId: args.Message.Chat.Id,
                            text: "You're now the agent of the agency: <b>" + agency.Name + "</b>.",
                            parseMode: ParseMode.Html
                        ).Wait();
                        collector.Finish();
                        return;
                    }

                    dbUser.Permission = Permission.Guest;
                    Context.BotService.Client.SendTextMessageAsync(
                        chatId: args.Message.Chat.Id,
                        text: "Invalid agency registration key."
                    ).Wait();
                    collector.Finish();
                    return;
                }

                string formattedText = args.Message.Text.Trim().ToLower();
                switch (formattedText)
                {
                    case UserRoles.Customer:
                        dbUser.Permission = Permission.User;
                        Context.BotService.Client.SendTextMessageAsync(
                            chatId: args.Message.Chat.Id,
                            text: "You are now registered as a customer."
                        ).Wait();
                        break;

                    case UserRoles.Agent:
                        dbUser.Permission = Permission.Agent;
                        Context.BotService.Client.SendTextMessageAsync(
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
                    Context.BotService.Client.SendTextMessageAsync(
                        chatId: Context.Message.Chat.Id,
                        text: "Signing up failed. Please, try again.",
                        replyMarkup: new ReplyKeyboardRemove()
                    ).Wait();
                    return;
                }

                var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                ctx.Users.Add(dbUser);
                ctx.SaveChanges();
                Context.BotService.Client.SendTextMessageAsync(
                    chatId: Context.Message.Chat.Id,
                    text: GenerateMenuMessage(Context.Message, ctx),
                    parseMode: ParseMode.Html,
                    replyMarkup: Context.CommandService.GetMenuKeyboard(dbUser.Permission)
                    ).Wait();
            };

            collector.OnMessageTimeout += (sender, args) =>
            {
                Context.BotService.Client.SendTextMessageAsync(
                        chatId: Context.Message.Chat.Id,
                        text: "The time is out, please, try again.",
                        replyMarkup: Context.CommandService.GetMenuKeyboard(dbUser.Permission)
                    ).Wait();
            };

            collector.Start();
            return Task.CompletedTask;
        }

        private async Task SendRoleQuestionAsync(long chatId)
        {
            await Context.BotService.Client.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Who are you?",
                        replyMarkup: Markups.ClientAgentButtons()
                    );
        }
    }
}