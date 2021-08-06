using System.Collections.Generic;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands.Metadata
{
    public class BotStatus
    {
        public bool IsWorking { get; set; }
        public User Bot { get; set; }
        public string Host { get; set; }
        public int CommandsLoaded { get; set; }
        public IEnumerable<CommandInfo> Commands { get; set; }
    }
}