using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.DatabaseExtensions;
using Masya.TelegramBot.DatabaseExtensions.Abstractions;
using Telegram.Bot;
using System.Linq;
using System.Threading.Tasks;

namespace Masya.TelegramBot.Modules
{
    public sealed class SearchCallbacksHandlerModule : DatabaseModule
    {
        private readonly IKeyboardGenerator _keyboards;

        public SearchCallbacksHandlerModule(IKeyboardGenerator keyboards)
        {
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