using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Metadata;
using Masya.TelegramBot.Commands.Options;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands.Abstractions
{
    public interface ICommandService<TCommandInfo, TAliasInfo>
        where TAliasInfo : AliasInfo
        where TCommandInfo : CommandInfo<TAliasInfo>
    {
        CommandServiceOptions Options { get; }
        List<TCommandInfo> Commands { get; }
        IBotService<TCommandInfo, TAliasInfo> BotService { get; }
        Task LoadCommandsAsync(Assembly assembly);
        Task ExecuteCommandAsync(Message message);
        bool CheckCommandCondition(TCommandInfo commandInfo, Message message);
        void HandleCallback(CallbackQuery callback);
    }
}