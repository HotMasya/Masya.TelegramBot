using System;

namespace Masya.TelegramBot.Commands.Attributes
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class CallbackAttribute : Attribute
    {
        public string CallbackData { get; }

        public CallbackAttribute(string callbackData)
        {
            CallbackData = callbackData;
        }
    }
}