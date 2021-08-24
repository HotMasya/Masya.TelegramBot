using Masya.TelegramBot.Commands.Metadata;
using Masya.TelegramBot.Commands.Options;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands.Abstractions
{
    public interface IBotService
    {
        ITelegramBotClient Client { get; }
        BotServiceOptions Options { get; }
        ICollector CreateMessageCollector(Chat chat, TimeSpan messageTimeout);
        Task SetWebhookAsync();
        Task HandleUpdateAsync(Update update);
        Task<BotStatus> GetStatusAsync();
    }
}