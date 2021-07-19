using Masya.TelegramBot.Commands.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands
{
    public sealed class DefaultCommandContext : ICommandContext
    {
        public IBotService BotService { get; }

        public Chat Chat { get; }

        public User User { get; }

        public Message Message { get; }

        public DefaultCommandContext(IBotService botService,
                                    Chat chat,
                                    User user,
                                    Message message)
        {
            BotService = botService;
            Chat = chat;
            User = user;
            Message = message;
        }
    }
}