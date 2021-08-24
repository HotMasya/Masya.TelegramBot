using System.ComponentModel.DataAnnotations;

namespace Masya.TelegramBot.DataAccess.Models
{
    public sealed class BotSettings
    {
        [Required]
        [MaxLength(50)]
        public string BotToken { get; set; }

        [Required]
        [MaxLength(256)]
        public string WebhookHost { get; set; }

        public bool IsEnabled { get; set; }
    }
}