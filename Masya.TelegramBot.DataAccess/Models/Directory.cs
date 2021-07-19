using System.ComponentModel.DataAnnotations;

namespace Masya.TelegramBot.DataAccess.Models
{
    public sealed class Directory
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }
    }
}
