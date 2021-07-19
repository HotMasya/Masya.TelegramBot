using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands.Abstractions
{
    public interface ICommandContext
    {
        IBotService BotService { get; }
        Chat Chat { get; }
        User User { get; }
        Message Message { get; }
    }
}