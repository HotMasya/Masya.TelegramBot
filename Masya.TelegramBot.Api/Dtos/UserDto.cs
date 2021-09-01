using System;
using System.ComponentModel.DataAnnotations;
using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class UserDto
    {
        [Required]
        public long Id { get; set; }

        public string AgencyName { get; set; }

        [Required]
        public Permission Permission { get; set; }

        [Required]
        public long TelegramAccountId { get; set; }

        [Required]
        public string TelegramLogin { get; set; }

        [Required]
        public byte[] TelegramAvatar { get; set; }

        [Required]
        public string TelegramFirstName { get; set; }

        public string TelegramLastName { get; set; }

        [Required]
        public string TelegramPhoneNumber { get; set; }

        [Required]
        public bool IsBlocked { get; set; }

        public string BlockReason { get; set; }

        public bool? IsBlockedByBot { get; set; }

        [Required]
        public bool IsIgnored { get; set; }

        public string Note { get; set; }
    }
}