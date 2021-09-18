using System;

namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class LogDto
    {
        public long Id { get; set; }
        public string Message { get; set; }
        public string Level { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}