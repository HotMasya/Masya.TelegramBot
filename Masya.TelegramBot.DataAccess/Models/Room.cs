using System.Collections.Generic;

namespace Masya.TelegramBot.DataAccess.Models
{
    public sealed class Room
    {
        public int Id { get; set; }
        public int RoomsCount { get; set; }

        public List<UserSettings> UserSettings { get; set; }
    }
}