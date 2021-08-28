using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masya.TelegramBot.DataAccess.Models
{
    [Table("Favorites")]
    public sealed class Favorites
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int PropertyObjectId { get; set; }
        public RealtyObject PropertyObject { get; set; }
    }
}
