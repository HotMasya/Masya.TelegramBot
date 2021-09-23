using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Masya.TelegramBot.DataAccess.Models
{
    public sealed class UserSettings
    {
        [Key]
        public long Id { get; set; }

        public int? MinPrice { get; set; }

        public int? MaxPrice { get; set; }

        public int? MinFloor { get; set; }

        public int? MaxFloor { get; set; }

        public int? MaxRoomsCount { get; set; }

        public List<DirectoryItem> SelectedRegions { get; set; }

        public List<Category> SelectedCategories { get; set; }
    }
}