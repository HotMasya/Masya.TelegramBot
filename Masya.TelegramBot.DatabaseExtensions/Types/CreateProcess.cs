using Masya.TelegramBot.DataAccess.Types;

namespace Masya.TelegramBot.DatabaseExtensions.Types
{
    public sealed class CreateProcess
    {
        public int? AgentId { get; set; }
        public SuperType? CategoryId { get; set; }
        public string Category { get; set; }
        public int? StreetId { get; set; }
        public string Street { get; set; }
        public int? DistrictId { get; set; }
        public string District { get; set; }
        public int? WallMaterialId { get; set; }
        public string WallMaterial { get; set; }
        public int? StateId { get; set; }
        public string State { get; set; }
        public int? Rooms { get; set; }
        public float? TotalArea { get; set; }
        public float? LivingSpace { get; set; }
        public float? KitchenSpace { get; set; }
        public float? LotArea { get; set; }
        public int? Floor { get; set; }
        public int? TotalFloors { get; set; }
        public int? Price { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
    }
}