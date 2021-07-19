using System;

namespace Masya.TelegramBot.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class ParseFormatAttribute : Attribute
    {
        public string Format { get; }
        public ParseFormatAttribute(string format)
        {
            Format = format;
        }
    }
}
