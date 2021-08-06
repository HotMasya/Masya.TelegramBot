using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Masya.TelegramBot.Commands.Data
{
    public sealed class Alias
    {
        [Key]
        [JsonIgnore]
        public int Id { get; set; }

        public string Name { get; set; }

        [ForeignKey("Command")]
        [JsonIgnore]
        public int CommandId { get; set; }

        [JsonIgnore]
        public Command Command { get; set; }
    }
}
