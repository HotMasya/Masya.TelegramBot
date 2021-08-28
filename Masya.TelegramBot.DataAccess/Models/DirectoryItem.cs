using System.ComponentModel.DataAnnotations;

namespace Masya.TelegramBot.DataAccess.Models
{
    public sealed class DirectoryItem
    {
        public int Id { get; set; }
        public int DirectoryId { get; set; }
        public int CategoryId { get; set; }

        [Required, MaxLength(256)]
        public string Value { get; set; }

        public Directory Directory { get; set; }
        public Category Category { get; set; }
    }
}
