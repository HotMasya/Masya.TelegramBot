using Masya.TelegramBot.Commands.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands.Abstractions
{
    public interface IBotService
    {
        ITelegramBotClient Client { get; }
        BotServiceOptions Options { get; }
        Task Run(CancellationToken cancellationToken = default);
        Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken = default);
        Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken = default);
    }
}