using System;

namespace Masya.TelegramBot.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AliasAttribute : Attribute
    {
        public string[] Aliases { get; }

        public AliasAttribute(params string[] aliases)
        {
            Aliases = aliases;
        }
    }
}