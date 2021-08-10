using System.ComponentModel.DataAnnotations;

namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class PhoneDto
    {
        [Phone]
        [Required]
        [MaxLength(20)]
        [MinLength(8)]
        public string PhoneNumber { get; set; }
    }
}