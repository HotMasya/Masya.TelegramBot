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

        public Permission? Permission { get; set; } = Data.Permission.All;

        [JsonIgnore]
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public Command ParentCommand { get; set; }

        [JsonIgnore]
        [ForeignKey("ParentId")]
        public List<Command> Aliases { get; set; }
    }
}
