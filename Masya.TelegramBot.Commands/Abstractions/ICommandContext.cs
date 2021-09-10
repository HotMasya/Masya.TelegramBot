using Masya.TelegramBot.Commands.Metadata;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands.Abstractions
{
    public interface ICommandContext<TCommandInfo, TAliasInfo>
        where TAliasInfo : AliasInfo
        where TCommandInfo : CommandInfo<TAliasInfo>
    {
        IBotService<TCommandInfo, TAliasInfo> BotService { get; }
        ICommandService<TCommandInfo, TAliasInfo> CommandService { get; }
        Chat Chat { get; }
        User User { get; }
        Message Message { get; }
    }
}