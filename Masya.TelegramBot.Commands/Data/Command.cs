using System.Collections.Generic;

namespace Masya.TelegramBot.Commands.Data
{
    public sealed class Command
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Permission Permission { get; set; }
        public List<Alias> Aliases { get; set; }
    }
}
