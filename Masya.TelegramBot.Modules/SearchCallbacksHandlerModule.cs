using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.DatabaseExtensions;
using Masya.TelegramBot.DatabaseExtensions.Abstractions;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Types;
using Telegram.Bot;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
            await Context.BotService.Client.EditMessageReplyMarkupAsync(
                chatId: Context.Message.Chat.Id,
                messageId: Context.Message.MessageId,
                replyMarkup: await _keyboards.InlineSearchAsync(Context.Callback.Data)
            );
        }

        [Callback(CallbackDataTypes.UpdateRegions)]
        public async Task HandleUpdateRegionsAsync()
        {
            var regions = await _keyboards.InlineSearchAsync(Context.Callback.Data);

            if (regions == null)
            {
                await Context.BotService.Client.AnswerCallbackQueryAsync(
                    callbackQueryId: Context.Callback.Id,
                    text: "There are no regions yet."
                );
                return;
            }
        }
    }
}