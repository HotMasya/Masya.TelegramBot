using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Masya.TelegramBot.Commands.Data;

namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class CommandDto
    {
        [MaxLength(32)]
        [Required]
        public string Name { get; set; }
        public Permission? Permission { get; set; }
        public bool? IsEnabled { get; set; }
        public bool? DisplayInMenu { get; set; }
        public IEnumerable<CommandDto> Aliases { get; set; }
    }
}