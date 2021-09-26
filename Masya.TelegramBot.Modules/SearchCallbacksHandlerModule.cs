using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.DatabaseExtensions;
using Masya.TelegramBot.DatabaseExtensions.Abstractions;
using Telegram.Bot;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using Masya.TelegramBot.DataAccess;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.Modules
{
    public sealed class SearchCallbacksHandlerModule : DatabaseModule
    {
        private readonly IKeyboardGenerator _keyboards;
        private readonly ILogger<SearchCallbacksHandlerModule> _logger;
        private readonly ApplicationDbContext _dbContext;

        public SearchCallbacksHandlerModule(
            IKeyboardGenerator keyboards,
            ILogger<SearchCallbacksHandlerModule> logger,
            ApplicationDbContext dbContext
        )
        {
            _dbContext = dbContext;
            _logger = logger;
            _keyboards = keyboards;
        }

        [Command("/search")]
        public async Task SearchAsync()
        {
            var user = _dbContext.Users
                .Include(u => u.UserSettings)
                    .ThenInclude(us => us.SelectedCategories)
                .Include(u => u.UserSettings)
                    .ThenInclude(us => us.SelectedRegions)
                .First(u => u.TelegramAccountId == Context.User.Id);

            if (user.UserSettings == null)
            {
                user.UserSettings = new UserSettings();
                await _dbContext.SaveChangesAsync();
            }

            await ReplyAsync(
                content: MessageGenerators.GenerateSearchSettingsMessage(user.UserSettings),
                parseMode: ParseMode.Markdown,
                replyMarkup: await _keyboards.InlineSearchAsync(CallbackDataTypes.SearchMenu)
            );
        }


        [Callback(CallbackDataTypes.SearchMenu)]
        public async Task HandleSearchMenuAsync()
        {
            await EditMessageAsync(
                replyMarkup: await _keyboards.InlineSearchAsync(CallbackDataTypes.SearchMenu)
            );
        }

        [Callback(CallbackDataTypes.ChangeSettings)]
        public async Task HandleChangeSettingsAsync()
        {
            await EditMessageAsync(
                replyMarkup: await _keyboards.InlineSearchAsync(CallbackDataTypes.ChangeSettings)
            );
        }

        [Callback(CallbackDataTypes.UpdateCategories)]
        public async Task HandleUpdateCategoriesAsync()
        {
            var categories = await _keyboards.InlineSearchAsync(CallbackDataTypes.UpdateCategories);
            if (!categories.InlineKeyboard.Any())
            {
                await Context.BotService.Client.AnswerCallbackQueryAsync(
                    callbackQueryId: Context.Callback.Id,
                    text: "There are no categories yet."
                );
                return;
            }

            await EditMessageAsync(replyMarkup: categories);
        }

        [Callback(CallbackDataTypes.UpdateRegions)]
        public async Task HandleUpdateRegionsAsync()
        {
            var regions = await _keyboards.InlineSearchAsync(CallbackDataTypes.UpdateRegions);
            if (!regions.InlineKeyboard.Any())
            {
                await Context.BotService.Client.AnswerCallbackQueryAsync(
                    callbackQueryId: Context.Callback.Id,
                    text: "There are no regions yet."
                );
                return;
            }

            await EditMessageAsync(replyMarkup: regions);
        }
    }
}