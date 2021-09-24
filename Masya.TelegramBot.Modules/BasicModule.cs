using System;
using System.Linq;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.Commands.Options;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Types;
using Masya.TelegramBot.DataAccess.Models;
using Masya.TelegramBot.DatabaseExtensions;
using Masya.TelegramBot.DatabaseExtensions.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;

namespace Masya.TelegramBot.Modules
{
    public sealed class BasicModule : DatabaseModule
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly CommandServiceOptions _options;
        private readonly IServiceProvider _services;
        private readonly IKeyboardGenerator _keyboards;

        public BasicModule(
            ApplicationDbContext context,
            IServiceProvider services,
            IOptions<CommandServiceOptions> options,
            IKeyboardGenerator keyboards
            )
        {
            _dbContext = context;
            _options = options.Value;
            _services = services;
            _keyboards = keyboards;
        }

        [Command("/start")]
        public async Task StartCommandAsync()
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.TelegramAccountId == Context.User.Id);
            if (user is null)
            {
                await ReplyAsync(
                    "First, you have to sign up.",
                    replyMarkup: new ReplyKeyboardMarkup(
                        KeyboardButton.WithRequestContact("Send information about me.")
                    )
                    { ResizeKeyboard = true }
                );
                return;
            }

            await ReplyAsync(
                content: MessageGenerators.GenerateMenuMessage(user),
                replyMarkup: _keyboards.Menu(user.Permission),
                parseMode: ParseMode.Markdown
                );
        }

        [Command("/search")]
        public async Task SearchAsync()
        {
            var user = _dbContext.Users
                .Include(u => u.UserSettings)
                    .ThenInclude(us => us.SelectedCategories)
                .Include(u => u.UserSettings)
                    .ThenInclude(us => us.SelectedRegions)
                .First(u => u.TelegramAccountId == Context.Message.From.Id);

            if (user.UserSettings == null)
            {
                user.UserSettings = new UserSettings();
                await _dbContext.SaveChangesAsync();
            }

            await ReplyAsync(
                content: MessageGenerators.GenerateSearchSettingsMessage(user.UserSettings),
                parseMode: ParseMode.Markdown,
                replyMarkup: await _keyboards.InlineSearch()
            );
        }

        [RegisterUser]
        public Task RegisterUserAsync(Contact contact)
        {
            var user = Context.Message.From;

            if (!contact.UserId.HasValue || contact.UserId.Value != user.Id) return Task.CompletedTask;

            var dbUser = new DataAccess.Models.User()
            {
                TelegramAccountId = contact.UserId.Value,
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
                        dbUser.TelegramAvatar = Context.BotService.Client
                            .DownloadAvatarAsync(contact.UserId.Value).GetAwaiter().GetResult();
                        Context.BotService.Client.SendTextMessageAsync(
                            chatId: args.Message.Chat.Id,
                            text: string.Format("You're now the agent of the agency: *{0}*.", agency.Name),
                            parseMode: ParseMode.Markdown
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

                string formattedText = args.Message.Text?.Trim()?.ToLower();
                switch (formattedText)
                {
                    case UserRoles.Customer:
                        dbUser.Permission = Permission.User;
                        dbUser.TelegramAvatar = Context.BotService.Client
                            .DownloadAvatarAsync(contact.UserId.Value).GetAwaiter().GetResult();
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
                    text: MessageGenerators.GenerateMenuMessage(dbUser),
                    parseMode: ParseMode.Markdown,
                    replyMarkup: _keyboards.Menu(dbUser.Permission)
                    ).Wait();
            };

            collector.OnMessageTimeout += (sender, args) =>
            {
                Context.BotService.Client.SendTextMessageAsync(
                        chatId: Context.Message.Chat.Id,
                        text: "The time is out, please, try again.",
                        replyMarkup: _keyboards.Menu(dbUser.Permission)
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
                        replyMarkup: _keyboards.Roles()
                    );
        }
    }
}