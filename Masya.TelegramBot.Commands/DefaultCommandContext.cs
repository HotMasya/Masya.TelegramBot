using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Commands.Metadata;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands
{
    public sealed class DefaultCommandContext<TCommandInfo, TAliasInfo> : ICommandContext<TCommandInfo, TAliasInfo>
        where TAliasInfo : AliasInfo
        where TCommandInfo : CommandInfo<TAliasInfo>
    {
        public IBotService<TCommandInfo, TAliasInfo> BotService { get; }

        public ICommandService<TCommandInfo, TAliasInfo> CommandService { get; }

        public Chat Chat { get; }

        public User User { get; }

        public Message Message { get; }

        public DefaultCommandContext(
            IBotService<TCommandInfo, TAliasInfo> botService,
            ICommandService<TCommandInfo, TAliasInfo> commandService,
            Chat chat,
            User user,
            Message message
        )
        {
            BotService = botService;
            CommandService = commandService;
            Chat = chat;
            User = user;
            Message = message;
        }
    }
}