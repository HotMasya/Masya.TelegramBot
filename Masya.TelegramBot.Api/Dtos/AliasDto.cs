using System.ComponentModel.DataAnnotations;
using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class AliasDto
    {
        [MaxLength(32)]
        [Required]
        public string Name { get; set; }
        public Permission Permission { get; set; }
        public bool? IsEnabled { get; set; }
        public bool? DisplayInMenu { get; set; }
        public int ParentId { get; set; }
    }
}