using Masya.TelegramBot.DataAccess.Types;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Masya.TelegramBot.DataAccess.Models
{
    public sealed class Category
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public SuperType SuperType { get; set; }

        [JsonIgnore]
        public List<UserSettings> UserSettings { get; set; }
    }
}
