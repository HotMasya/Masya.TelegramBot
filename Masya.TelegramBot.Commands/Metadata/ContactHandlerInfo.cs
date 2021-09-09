using System.Reflection;

namespace Masya.TelegramBot.Commands.Metadata
{
    public sealed class ContactHandlerInfo
    {
        public MethodInfo MethodInfo { get; }

        public ContactHandlerInfo(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
        }
    }
}