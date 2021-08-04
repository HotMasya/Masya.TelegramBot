using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masya.TelegramBot.Commands.Data
{
    public sealed class Alias
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [ForeignKey("Command")]
        public int CommandId { get; set; }

        public Command Command { get; set; }
    }
}
