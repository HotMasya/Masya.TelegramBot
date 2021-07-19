using System.ComponentModel.DataAnnotations;

namespace Masya.TelegramBot.DataAccess.Models
{
    public sealed class Reference
    {
        [Key]
        public int Id { get; set; }

        public int ReferenceId { get; set; }

        public int AgencyId { get; set; }

        public Agency Agency { get; set; }
    }
}
