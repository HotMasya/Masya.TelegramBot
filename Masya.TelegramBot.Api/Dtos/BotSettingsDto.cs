
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class BotSettingsDto
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string WebhookHost { get; set; }
        public bool? IsEnabled { get; set; }
        public User BotUser { get; set; }
    }
}