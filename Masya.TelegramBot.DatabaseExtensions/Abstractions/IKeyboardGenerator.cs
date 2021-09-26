using System;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Options;
using Masya.TelegramBot.DataAccess.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace Masya.TelegramBot.DatabaseExtensions.Abstractions
{
    public interface IKeyboardGenerator
    {
        IServiceProvider Services { get; }
        CommandServiceOptions Options { get; }
        IReplyMarkup Menu(Permission userPermission);
        Task<InlineKeyboardMarkup> InlineSearch(string callbackDataType = null);
        IReplyMarkup Roles();
    }
}