using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Modules;
using System.Threading.Tasks;

namespace Masya.TelegramBot.Api.Bot
{
    public class BotSetup
    {
        private readonly ICommandService _commands;
        private readonly IBotService _bot;

        public BotSetup(ICommandService commands, IBotService bot)
        {
            _commands = commands;
            _bot = bot;
        }

        public async Task SetupAsync()
        {
            await _commands.LoadCommandsAsync(typeof(BasicModule).Assembly);
            await _bot.RunAsync();
        }
    }
}
