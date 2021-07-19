using Masya.TelegramBot.DataAccess.Enums;
using System.ComponentModel.DataAnnotations;

namespace Masya.TelegramBot.DataAccess.Models
{
    public sealed class Category
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public SuperType SuperType { get; set; }
    }
}
