namespace Masya.TelegramBot.Commands.Options
{
    public sealed class BotServiceOptions
    {
        public string Token { get; set; }
        public string WebhookHost { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}