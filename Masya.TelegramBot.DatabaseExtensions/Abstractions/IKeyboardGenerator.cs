using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Options;
using Masya.TelegramBot.DataAccess.Models;
using Masya.TelegramBot.DataAccess.Types;
using Masya.TelegramBot.DatabaseExtensions.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Masya.TelegramBot.DatabaseExtensions.Abstractions
{
    public interface IKeyboardGenerator
    {
        CommandServiceOptions Options { get; }
        IReplyMarkup Menu(Permission userPermission);
        Task<InlineKeyboardMarkup> InlineSearchAsync(string callbackDataType = null, UserSettings userSettings = null);
        Task<InlineKeyboardMarkup> SelectCategoriesAsync();
        InlineKeyboardMarkup ShowCreationMenu(CreateProcess process, bool isEditMode = false);
        Task<InlineKeyboardMarkup> SearchStreetsResults(string query);
        Task<InlineKeyboardMarkup> ShowRegionsAsync();
        Task<InlineKeyboardMarkup> SelectDirectoryItems(DirectoryType type, string prefix);
        InlineKeyboardMarkup SelectNumericValues(string valuesButtonData, int maxValue, string prefix);
    }
}