using System.Reflection;

namespace Masya.TelegramBot.Commands.Metadata
{
    public sealed class CallbackInfo
    {
        public string CallbackData { get; set; }
        public MethodInfo Handler { get; set; }
    }
}