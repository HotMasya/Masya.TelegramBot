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

        private string GenerateMenuMessage(Message message)
        {
            var user = _dbContext.Users.First(u => u.TelegramAccountId == message.From.Id);
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
            if (!_dbContext.Users.Any(u => u.TelegramAccountId == Context.User.Id))
            {
                await ReplyAsync(
                    "First, you have to sign up.",
                    replyMarkup: new ReplyKeyboardMarkup(KeyboardButton.WithRequestContact("Send information about me."))
                );
                return;
            }
            await ReplyAsync(GenerateMenuMessage(Context.Message), replyMarkup: Context.CommandService.GetMenuKeyboard());
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

            string resultText = "";
            string password = null;
            var scope = _services.CreateScope();
            collector.OnStart += (sender, args) =>
            {
                SendRoleQuestionAsync(Context.Message.Chat.Id).Wait();
            };

            collector.OnMessageReceived += (sender, args) =>
            {
                if (dbUser.Permission.HasValue)
                {
                    if (password is null)
                    {
                        Context.BotService.Client.SendTextMessageAsync(
                            chatId: args.Message.Chat.Id,
                            text: "Enter agency registration key, please."
                        );
                        return;
                    }

                    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var agency = ctx.Agencies.FirstOrDefault(a => a.RegistrationKey.Equals(password));

                    if (agency != null)
                    {
                        dbUser.AgencyId = agency.Id;
                        collector.Finish();
                        return;
                    }

                    dbUser.Permission = null;
                    password = null;
                    Context.BotService.Client.SendTextMessageAsync(
                        chatId: args.Message.Chat.Id,
                        text: "Invalid agency registration key."
                    );
                    SendRoleQuestionAsync(args.Message.Chat.Id).Wait();
                    return;
                }

                string formattedText = args.Message.Text.Trim().ToLower();
                switch (formattedText)
                {
                    case UserRoles.Customer:
                        dbUser.Permission = Permission.User;
                        resultText = "You're now registered as a customer.";
                        break;

                    case UserRoles.Agent:
                        dbUser.Permission = Permission.Agent;
                        break;

                    default:
                        SendRoleQuestionAsync(args.Message.Chat.Id).Wait();
                        return;
                }
                collector.Finish();
            };

            collector.OnFinish += (sender, args) =>
            {
                var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                ctx.Users.Add(dbUser);
                ctx.SaveChanges();
                scope.Dispose();
                Context.BotService.Client.SendTextMessageAsync(
                    chatId: Context.Message.Chat.Id,
                    text: resultText,
                    replyMarkup: Context.CommandService.GetMenuKeyboard()
                    ).Wait();
            };

            collector.OnMessageTimeout += (sender, args) =>
            {
                Context.BotService.Client.SendTextMessageAsync(
                        chatId: Context.Message.Chat.Id,
                        text: "The time is out, please, try again."
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