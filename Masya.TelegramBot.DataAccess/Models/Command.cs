using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Masya.TelegramBot.DataAccess.Models
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

        public Permission? Permission { get; set; } = Models.Permission.User;

        public int? ParentId { get; set; }

        [JsonIgnore]
        public Command ParentCommand { get; set; }

        [JsonIgnore]
        public List<Command> Aliases { get; set; }

        public Command()
        {
            Aliases = new List<Command>();
        }
    }
}
