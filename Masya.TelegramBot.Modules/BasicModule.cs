using System.Linq;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Models;
using Masya.TelegramBot.DatabaseExtensions;
using Masya.TelegramBot.DatabaseExtensions.Abstractions;
using Masya.TelegramBot.DatabaseExtensions.Utils;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

namespace Masya.TelegramBot.Modules
{
    public sealed class BasicModule : DatabaseModule
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IKeyboardGenerator _keyboards;

        public BasicModule(ApplicationDbContext context, IKeyboardGenerator keyboards)
        {
            _dbContext = context;
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
                        KeyboardButton.WithRequestContact("👤 Share my contact")
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
    }
}