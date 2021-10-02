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

        private User GetUser()
        {
            return _dbContext.Users
                .Include(u => u.UserSettings)
                    .ThenInclude(us => us.SelectedCategories)
                .Include(u => u.UserSettings)
                    .ThenInclude(us => us.SelectedRegions)
                .FirstOrDefault(u => u.TelegramAccountId == Context.User.Id);
        }

        [Command("/search")]
        public async Task SearchAsync()
        {
            var user = GetUser();

            if (user == null)
            {
                return;
            }

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

        [Callback(CallbackDataTypes.UpdatePrice)]
        public async Task HandleUpdatePriceAsync(string type = null, int selectedValue = -1)
        {
            var user = GetUser();

            if (selectedValue != -1 && !string.IsNullOrEmpty(type))
            {
                switch (type)
                {
                    case KeyboardGenerator.MaxOperation:
                        user.UserSettings.MaxPrice = selectedValue;
                        break;

                    case KeyboardGenerator.MinOperation:
                        user.UserSettings.MinPrice = selectedValue;
                        break;

                    default: break;
                }
            }

            var prices = await _keyboards.InlineSearchAsync(CallbackDataTypes.UpdatePrice, user.UserSettings);

            if (!prices.InlineKeyboard.Any())
            {
                await Context.BotService.Client.AnswerCallbackQueryAsync(
                    callbackQueryId: Context.Callback.Id,
                    text: "There are no prices buttons yet."
                );
                return;
            }

            await EditMessageAsync(
                text: !string.IsNullOrEmpty(type) && selectedValue != -1 ? MessageGenerators.GenerateSearchSettingsMessage(user.UserSettings) : null,
                replyMarkup: prices,
                parseMode: ParseMode.Markdown
            );
        }

        [Callback(CallbackDataTypes.UpdatePrice)]
        public async Task HandleUpdateFloorsAsync(string type = null, int selectedValue = -1)
        {
            var user = GetUser();

            if (selectedValue != -1 && !string.IsNullOrEmpty(type))
            {
                switch (type)
                {
                    case KeyboardGenerator.MaxOperation:
                        user.UserSettings.MaxFloor = selectedValue;
                        break;

                    case KeyboardGenerator.MinOperation:
                        user.UserSettings.MinFloor = selectedValue;
                        break;

                    default: break;
                }
            }

            var floors = await _keyboards.InlineSearchAsync(CallbackDataTypes.UpdateFloors, user.UserSettings);

            if (!floors.InlineKeyboard.Any())
            {
                await Context.BotService.Client.AnswerCallbackQueryAsync(
                    callbackQueryId: Context.Callback.Id,
                    text: "There are no floors buttons yet."
                );
                return;
            }

            await EditMessageAsync(
                text: !string.IsNullOrEmpty(type) && selectedValue != -1 ? MessageGenerators.GenerateSearchSettingsMessage(user.UserSettings) : null,
                replyMarkup: floors,
                parseMode: ParseMode.Markdown
            );
        }

        [Callback(CallbackDataTypes.UpdateCategories)]
        public async Task HandleUpdateCategoriesAsync(int categoryId = -1)
        {
            var user = GetUser();

            if (user == null)
            {
                return;
            }

            if (categoryId != -1)
            {
                var selectedCategory = user.UserSettings.SelectedCategories.FirstOrDefault(c => c.Id == categoryId);

                if (selectedCategory == null)
                {
                    user.UserSettings.SelectedCategories.Add(
                        _dbContext.Categories.First(c => c.Id == categoryId)
                    );
                }
                else
                {
                    user.UserSettings.SelectedCategories.Remove(selectedCategory);
                }
                await _dbContext.SaveChangesAsync();
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
                replyMarkup: categories,
                parseMode: ParseMode.Markdown
            );
        }

        [Callback(CallbackDataTypes.UpdateRegions)]
        public async Task HandleUpdateRegionsAsync(int regionId = -1)
        {
            var user = GetUser();

            if (user == null)
            {
                return;
            }

            if (regionId != -1)
            {
                var selectedRegion = user.UserSettings.SelectedRegions.FirstOrDefault(c => c.Id == regionId);

                if (selectedRegion == null)
                {
                    user.UserSettings.SelectedRegions.Add(
                        _dbContext.DirectoryItems.First(c => c.Id == regionId)
                    );
                }
                else
                {
                    user.UserSettings.SelectedRegions.Remove(selectedRegion);
                }
                await _dbContext.SaveChangesAsync();
            }

            var regions = await _keyboards.InlineSearchAsync(
                CallbackDataTypes.UpdateRegions, user.UserSettings
            );

            if (!regions.InlineKeyboard.Any())
            {
                await Context.BotService.Client.AnswerCallbackQueryAsync(
                    callbackQueryId: Context.Callback.Id,
                    text: "There are no regions yet."
                );
                return;
            }

            await EditMessageAsync(
                text: regionId != -1 ? MessageGenerators.GenerateSearchSettingsMessage(user.UserSettings) : null,
                replyMarkup: regions,
                parseMode: ParseMode.Markdown
            );
        }
    }
}