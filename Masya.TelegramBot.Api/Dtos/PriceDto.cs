namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class PriceDto
    {
        public int? Id { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
    }
}