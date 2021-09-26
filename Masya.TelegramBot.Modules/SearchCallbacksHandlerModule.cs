using Masya.TelegramBot.DatabaseExtensions;
using Masya.TelegramBot.Commands.Attributes;
using System.Threading.Tasks;
using Masya.TelegramBot.DatabaseExtensions.Abstractions;
using Telegram.Bot;

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
        public async Task HandleUpdateSettingsAsync()
        {
            await Context.BotService.Client.EditMessageReplyMarkupAsync(
                chatId: Context.Message.Chat.Id,
                messageId: Context.Message.MessageId,
                replyMarkup: await _keyboards.InlineSearch(Context.Callback.Data)
            );
        }
    }
}