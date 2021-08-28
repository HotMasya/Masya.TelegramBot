using System.ComponentModel.DataAnnotations;

namespace Masya.TelegramBot.DataAccess.Models
{
    public sealed class Report
    {
        [Key]
        public int Id { get; set; }

        public int PropertyObjectId { get; set; }
        public RealtyObject PropertyObject { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        [MaxLength(256)]
        public string Text { get; set; }
    }
}
