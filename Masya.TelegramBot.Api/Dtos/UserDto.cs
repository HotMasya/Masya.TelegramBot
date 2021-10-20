using System.ComponentModel.DataAnnotations;
using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class UserDto
    {
        public long Id { get; set; }

        public int? AgencyId { get; set; }

        [Required]
        public Permission Permission { get; set; }

        public long TelegramAccountId { get; set; }

        [Required]
        public string TelegramLogin { get; set; }

        [Required]
        public string TelegramAvatar { get; set; }

        [Required]
        public string TelegramFirstName { get; set; }

        public string TelegramLastName { get; set; }

        [Required]
        public string TelegramPhoneNumber { get; set; }

        public bool IsBlocked { get; set; }

        public string BlockReason { get; set; }

        public bool? IsBlockedByBot { get; set; }

        public bool IsIgnored { get; set; }

        public string Note { get; set; }
    }
}