using Masya.TelegramBot.DatabaseExtensions;
using Masya.TelegramBot.Commands.Attributes;
using System.Threading.Tasks;

namespace Masya.TelegramBot.Modules
{
    public sealed class SearchCallbacksHandlerModule : DatabaseModule
    {
        [Callback(CallbackDataTypes.ExecuteSearch)]
        public async Task HandleSearchAsync()
        {
            await ReplyAsync("You have selected search.");
        }
    }
}