using System;
using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class AccountDto
    {
        public long Id { get; set; }
        public long TelegramAccountId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Avatar { get; set; }
        public string PhoneNumber { get; set; }
        public Permission Permission { get; set; }

        public AccountDto(User user)
        {
            Id = user.Id;
            FirstName = user.TelegramFirstName;
            LastName = user.TelegramLastName;
            TelegramAccountId = user.TelegramAccountId;
            Permission = user.Permission;
            PhoneNumber = user.TelegramPhoneNumber;
            if (user.TelegramAvatar != null && user.TelegramAvatar.Length > 0)
            {
                Avatar = Convert.ToBase64String(user.TelegramAvatar);
            }
        }
    }
}