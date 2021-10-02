namespace Masya.TelegramBot.DataAccess.Models
{
    public sealed class Image
    {
        public long Id { get; set; }
        public string Url { get; set; }
        public int RealtyObjectId { get; set; }
    }
}