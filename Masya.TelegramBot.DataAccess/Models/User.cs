using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masya.TelegramBot.DataAccess.Models
{
    public sealed class User
    {
        [Key]
        public long Id { get; set; }

        public int? AgencyId { get; set; }
        public Agency Agency { get; set; }

        public Permission Permission { get; set; }

        public long TelegramAccountId { get; set; }

        [MaxLength(32)]
        [MinLength(5)]
        public string TelegramLogin { get; set; }

        public byte[] TelegramAvatar { get; set; }

        [MaxLength(64)]
        public string TelegramFirstName { get; set; }

        [MaxLength(64)]
        public string TelegramLastName { get; set; }

        [MaxLength(20)]
        public string TelegramPhoneNumber { get; set; }

        public DateTime? LastCalledAt { get; set; }

        public bool IsBlocked { get; set; }

        [MaxLength(255)]
        public string BlockReason { get; set; }

        public bool? IsBlockedByBot { get; set; }

        public bool IsIgnored { get; set; }

        [MaxLength(255)]
        public string Note { get; set; }

        [ForeignKey("AgentId")]
        public List<RealtyObject> PropertyObjects { get; set; }

        public override string ToString()
        {
            return string.Format(
                "{0} - {1} {2} @{3}, Permission: {4}",
                Id,
                TelegramFirstName,
                TelegramLastName,
                TelegramLogin,
                Permission.ToString());
        }
    }
}
