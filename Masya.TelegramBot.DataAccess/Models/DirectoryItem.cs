namespace Masya.TelegramBot.DataAccess.Models
{
    public sealed class DirectoryItem
    {
        public int Id { get; set; }
        public int DirectoryId { get; set; }
        public int CategoryId { get; set; }

        public Directory Directory { get; set; }
        public Category Category { get; set; }
    }
}
