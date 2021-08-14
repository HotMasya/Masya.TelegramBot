using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Masya.TelegramBot.Commands.Data
{
    public sealed class Command
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public bool DisplayInMenu { get; set; }
        public Permission Permission { get; set; }

        [JsonIgnore]
        public List<Command> Aliases { get; set; }
    }
}
