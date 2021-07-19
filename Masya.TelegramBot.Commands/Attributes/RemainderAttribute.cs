using System;

namespace Masya.TelegramBot.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class RemainderAttribute : Attribute
    { }
}