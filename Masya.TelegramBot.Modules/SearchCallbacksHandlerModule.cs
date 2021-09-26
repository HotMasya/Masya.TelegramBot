using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.DatabaseExtensions;
using Masya.TelegramBot.DatabaseExtensions.Abstractions;
using Telegram.Bot;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace Masya.TelegramBot.Modules
{
    public sealed class SearchCallbacksHandlerModule : DatabaseModule
    {
        private readonly IKeyboardGenerator _keyboards;
        private readonly ILogger<SearchCallbacksHandlerModule> _logger;

        public SearchCallbacksHandlerModule(
            IKeyboardGenerator keyboards,
            ILogger<SearchCallbacksHandlerModule> logger
        )
        {
            _logger = logger;
            _keyboards = keyboards;
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
            try
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
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString());
            }
        }
    }
}