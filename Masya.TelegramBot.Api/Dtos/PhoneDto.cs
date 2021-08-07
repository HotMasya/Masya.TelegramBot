using System.ComponentModel.DataAnnotations;

namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class PhoneDto
    {
        [Phone]
        [Required]
        public string PhoneNumber { get; set; }
    }
}