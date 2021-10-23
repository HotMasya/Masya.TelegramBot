using System;

namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class RealtyObjectDto
    {
        public int? Id { get; set; }
        public int? InternalId { get; set; }
        public int? StreetId { get; set; }
        public int? DistrictId { get; set; }
        public int? WallMaterialId { get; set; }
        public int? StateId { get; set; }
        public long? AgentId { get; set; }
        public int CategoryId { get; set; }
        public float? TotalArea { get; set; }
        public float? LivingSpace { get; set; }
        public float? KitchenSpace { get; set; }
        public float? LotArea { get; set; }
        public int? Floor { get; set; }
        public int? TotalFloors { get; set; }
        public int? Rooms { get; set; }
        public int? Price { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime EditedAt { get; set; }
    }
}