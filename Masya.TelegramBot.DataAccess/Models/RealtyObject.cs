using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masya.TelegramBot.DataAccess.Models
{
    public sealed class RealtyObject
    {
        [Key]
        public int Id { get; set; }

        public int? InternalId { get; set; }

        [ForeignKey("Type")]
        public int? TypeId { get; set; }
        public DirectoryItem Type { get; set; }

        [ForeignKey("Street")]
        public int? StreetId { get; set; }
        public DirectoryItem Street { get; set; }

        [ForeignKey("District")]
        public int? DistrictId { get; set; }
        public DirectoryItem District { get; set; }

        [ForeignKey("WallMaterial")]
        public int? WallMaterialId { get; set; }
        public DirectoryItem WallMaterial { get; set; }

        [ForeignKey("State")]
        public int? StateId { get; set; }
        public DirectoryItem State { get; set; }

        public int AgentId { get; set; }
        public User Agent { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public float TotalArea { get; set; }
        public float LivingSpace { get; set; }
        public float KitchenSpace { get; set; }
        public float LotArea { get; set; }

        public int? Floor { get; set; }
        public int? TotalFloors { get; set; }

        [MaxLength(512)]
        public string Description { get; set; }

        public DateTime? MailingDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime EditedAt { get; set; }
    }
}
