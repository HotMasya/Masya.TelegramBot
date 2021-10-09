using System;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Options;
using Masya.TelegramBot.DataAccess.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace Masya.TelegramBot.DatabaseExtensions.Abstractions
{
    public interface IKeyboardGenerator
    {
        CommandServiceOptions Options { get; }
        IReplyMarkup Menu(Permission userPermission);
        Task<InlineKeyboardMarkup> InlineSearchAsync(string callbackDataType = null, UserSettings userSettings = null);
    }
}