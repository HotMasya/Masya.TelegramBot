using Masya.TelegramBot.Commands.Metadata;
using Masya.TelegramBot.Commands.Options;
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands.Abstractions
{
    public interface IBotService<TCommandInfo, TAliasInfo>
        where TAliasInfo : AliasInfo
        where TCommandInfo : CommandInfo<TAliasInfo>
    {
        ITelegramBotClient Client { get; }
        BotServiceOptions Options { get; }
        void LoadBot();
        ICollector<TCommandInfo, TAliasInfo> CreateMessageCollector(Chat chat, TimeSpan messageTimeout);
        Task SetWebhookAsync();
        Task HandleUpdateAsync(Update update);
        Task<BotStatus> GetSettingsAsync();
        Task<bool> TestSettingsAsync(string token, string webhookHost);
        void TryRemoveCollector(Chat chat);
    }
}