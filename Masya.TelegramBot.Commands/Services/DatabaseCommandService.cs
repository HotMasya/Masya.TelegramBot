using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Commands.Data;
using Masya.TelegramBot.Commands.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Masya.TelegramBot.Commands.Services
{
    public class DatabaseCommandService : DefaultCommandService
    {
        private readonly CommandDbContext _dbContext;

        public DatabaseCommandService(
            CommandDbContext context,
            IOptionsMonitor<CommandServiceOptions> options,
            IBotService botService,
            IServiceProvider services,
            ILogger<DefaultCommandService> logger
            )
            : base(options, botService, services, logger)
        {
            _dbContext = context;
        }

        public override async Task LoadCommandsAsync(Assembly assembly)
        {
            await base.LoadCommandsAsync(assembly);
            await _dbContext.AttachCommandsAsync(commands);
        }
    }
}
