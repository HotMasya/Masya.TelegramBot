namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class AgentDto
    {
        public string TelegramLogin { get; set; }
        public string TelegramAvatar { get; set; }
        public string TelegramFirstName { get; set; }
        public string TelegramLastName { get; set; }
        public string TelegramPhoneNumber { get; set; }
        public bool IsBlocked { get; set; }
        public string BlockReason { get; set; }
        public bool? IsBlockedByBot { get; set; }
        public bool IsIgnored { get; set; }
        public string Note { get; set; }
    }
}