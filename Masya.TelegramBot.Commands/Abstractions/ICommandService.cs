using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Options;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands.Abstractions
{
    public interface ICommandService
    {
        CommandServiceOptions Options { get; }
        IBotService BotService { get; }
        Task LoadModulesAsync(Assembly assembly);
        Task ExecuteCommandAsync(Message message);
        bool TryAddStepMessage(Message message);
    }
}