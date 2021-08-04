using System;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands.Events
{
    public class CollectorEventArgs: EventArgs
    {
        public Message Message { get; }
        
        public CollectorEventArgs(Message message)
        {
            Message = message;
        }
    }
}
