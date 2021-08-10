using System.ComponentModel.DataAnnotations;

namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class CodeDto
    {
        [Required]
        [MaxLength(6)]
        [MinLength(6)]
        public string Code { get; set; }

        [Phone]
        [Required]
        [MaxLength(20)]
        [MinLength(8)]
        public string PhoneNumber { get; set; }
    }
}