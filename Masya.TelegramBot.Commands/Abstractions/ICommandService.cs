using System.Reflection;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Options;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands.Abstractions
{

    public interface ICommandService
    {
        CommandServiceOptions Options { get; }
        IBotService BotService { get; }
        Task LoadCommandsAsync(Assembly assembly);
        Task ExecuteCommandAsync(Message message);
    }
}