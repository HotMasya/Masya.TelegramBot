using System.Collections.Generic;
using System.Threading.Tasks;
using Masya.TelegramBot.Api.Dtos;
using Microsoft.Extensions.Configuration;

namespace Masya.TelegramBot.Api.Services.Abstractions
{
    public interface IDatabaseLogsService
    {
        IConfiguration Configuration { get; }

        Task<IEnumerable<LogDto>> GetBotLogsAsync(int? agencyId = null);
    }
}