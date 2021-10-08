using System.Collections.Generic;
using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.DatabaseExtensions
{
    public sealed class SearchProcess
    {
        public long TelegramId { get; set; }
        public IEnumerable<RealtyObject> RealtyObjects { get; set; }
        public int ItemsSentCount { get; set; }
        public int TotalItemsFound { get; set; }
    }
}