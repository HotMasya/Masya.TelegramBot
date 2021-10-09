using System.Linq;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DatabaseExtensions;
using Masya.TelegramBot.DatabaseExtensions.Abstractions;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;

namespace Masya.TelegramBot.Modules
{
    public sealed class CreateObjectModule : DatabaseModule
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IKeyboardGenerator _keyboards;

        public CreateObjectModule(
            ApplicationDbContext dbContext,
            IKeyboardGenerator keyboards
            )
        {
            _dbContext = dbContext;
            _keyboards = keyboards;
        }

        [Command("/myobj")]
        public async Task MyObjectsCommandAsync()
        {
            var objects = await _dbContext.RealtyObjects
                .Where(ro => ro.Agent.TelegramAccountId == Context.User.Id)
                .ToListAsync();

            if (objects == null || objects.Count == 0)
            {
                await ReplyAsync(
                    "❌ You haven't created any realty objects yet.\n\nUse /create command to start *realty object creating procedure*.",
                    ParseMode.Markdown
                );
            }

            await SendObjectsAsync(objects);
        }

        [Command("/create")]
        public async Task CreateObjectCommandAsync()
        {
            await ReplyAsync(
                content: "Ok. First of all you should select the *category* of your object.",
                replyMarkup: await _keyboards.SelectCategoriesAsync(),
                parseMode: ParseMode.Markdown
            );
        }
    }
}