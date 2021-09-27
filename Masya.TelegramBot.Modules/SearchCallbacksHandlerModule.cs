using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.DatabaseExtensions;
using Masya.TelegramBot.DatabaseExtensions.Abstractions;
using Telegram.Bot;
using System.Linq;
using System.Threading.Tasks;
using Masya.TelegramBot.DataAccess;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using Masya.TelegramBot.DataAccess.Models;
using Microsoft.Extensions.Logging;
using System;

namespace Masya.TelegramBot.Modules
{
    public sealed class SearchCallbacksHandlerModule : DatabaseModule
    {
        private readonly IKeyboardGenerator _keyboards;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<SearchCallbacksHandlerModule> _logger;

        public SearchCallbacksHandlerModule(
            IKeyboardGenerator keyboards,
            ApplicationDbContext dbContext,
            ILogger<SearchCallbacksHandlerModule> logger
        )
        {
            _dbContext = dbContext;
            _keyboards = keyboards;
            _logger = logger;
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
                replyMarkup: await _keyboards.InlineSearchAsync()
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
        public async Task HandleUpdateCategoriesAsync(int categoryId = -1)
        {
            try
            {
                var user = _dbContext.Users
                            .Include(u => u.UserSettings)
                                .ThenInclude(us => us.SelectedCategories)
                            .Include(u => u.UserSettings)
                                .ThenInclude(us => us.SelectedRegions)
                            .First(u => u.TelegramAccountId == Context.User.Id);

                if (categoryId != -1)
                {
                    if (user == null)
                    {
                        return;
                    }

                    var selectedCategory = user.UserSettings.SelectedCategories.FirstOrDefault(c => c.Id == categoryId);

                    if (selectedCategory == null)
                    {
                        user.UserSettings.SelectedCategories.Add(
                            _dbContext.Categories.First(c => c.Id == categoryId)
                        );
                        return;
                    }

                    user.UserSettings.SelectedCategories.Remove(selectedCategory);
                }

                var categories = await _keyboards.InlineSearchAsync(
                    CallbackDataTypes.UpdateCategories, user.UserSettings
                );

                if (!categories.InlineKeyboard.Any())
                {
                    await Context.BotService.Client.AnswerCallbackQueryAsync(
                        callbackQueryId: Context.Callback.Id,
                        text: "There are no categories yet."
                    );
                    return;
                }

                await EditMessageAsync(
                    text: categoryId != -1 ? MessageGenerators.GenerateSearchSettingsMessage(user.UserSettings) : null,
                    replyMarkup: categories
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
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