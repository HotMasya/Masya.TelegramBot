using System.ComponentModel.DataAnnotations;
using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class UserSaveDto
    {
        public long Id { get; set; }
        public Permission Permission { get; set; }
        public bool IsBlocked { get; set; }
        public string BlockReason { get; set; }
        public string Note { get; set; }
    }
}