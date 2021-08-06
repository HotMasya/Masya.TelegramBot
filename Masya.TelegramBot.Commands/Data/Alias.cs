using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Masya.TelegramBot.Commands.Data
{
    public sealed class Alias
    {
        [Key]
        [JsonIgnore]
        public int Id { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        [ForeignKey("Command")]
        public int CommandId { get; set; }

        [JsonIgnore]
        public Command Command { get; set; }
    }
}
