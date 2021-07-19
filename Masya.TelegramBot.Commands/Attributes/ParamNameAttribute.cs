using System;

namespace Masya.TelegramBot.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class ParamNameAttribute : Attribute
    {
        public string Name { get; }
        public ParamNameAttribute(string name)
        {
            Name = name;
        }
    }
}