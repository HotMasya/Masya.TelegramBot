using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Metadata;
using Masya.TelegramBot.Commands.Options;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands.Abstractions
{

    public interface ICommandService
    {
        CommandServiceOptions Options { get; }
        List<CommandInfo> Commands { get; }
        IBotService BotService { get; }
        Task LoadCommandsAsync(Assembly assembly);
        Task ExecuteCommandAsync(Message message);
    }
}