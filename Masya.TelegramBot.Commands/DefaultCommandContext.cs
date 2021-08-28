using Masya.TelegramBot.Commands.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands
{
    public sealed class DefaultCommandContext : ICommandContext
    {
        public IBotService BotService { get; }

        public ICommandService CommandService { get; }

        public Chat Chat { get; }

        public User User { get; }

        public Message Message { get; }

        public DefaultCommandContext(
            IBotService botService,
            ICommandService commandService,
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