using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class AccountDto
    {
        public long Id { get; set; }
        public long TelegramAccountId { get; set; }
        public string TelegramFirstName { get; set; }
        public string TelegramLastName { get; set; }
        public string TelegramAvatar { get; set; }
        public string TelegramPhoneNumber { get; set; }
        public Permission Permission { get; set; }
        public Agency Agency { get; set; }
    }
}