using System.Collections.Generic;

namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class ValuesDto
    {
        public List<PriceDto> Prices { get; set; }
        public List<FloorDto> Floors { get; set; }
        public List<RoomDto> Rooms { get; set; }
    }
}