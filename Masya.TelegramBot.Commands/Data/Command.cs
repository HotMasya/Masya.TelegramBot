using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Masya.TelegramBot.Commands.Data
{
    public sealed class Command
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(32)]
        [Required]
        public string Name { get; set; }

        public bool? IsEnabled { get; set; }

        public bool? DisplayInMenu { get; set; }

        public Permission? Permission { get; set; } = Data.Permission.User;

        [JsonIgnore]
        public int? ParentId { get; set; }
        public Command ParentCommand { get; set; }

        [JsonIgnore]
        public List<Command> Aliases { get; set; }

        public Command()
        {
            Aliases = new List<Command>();
        }
    }
}
