using System;

namespace Masya.TelegramBot.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class CommandAttribute : Attribute
    {
        public string Name { get; }

        public CommandAttribute(string name)
        {
            Name = name;
        }
    }
}