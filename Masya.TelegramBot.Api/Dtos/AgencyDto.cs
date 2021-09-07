using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class AgencyDto
    {
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [MaxLength(1024)]
        public string Description { get; set; }

        [MaxLength(128)]
        [Required]
        public string RegistrationKey { get; set; }

        public bool? IsRegWithoutAdmin { get; set; }

        [MaxLength(256)]
        public string ImportUrl { get; set; }

        public List<AgentDto> Agents { get; set; }
    }
}