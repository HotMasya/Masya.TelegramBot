using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Metadata;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Masya.TelegramBot.Commands.Abstractions
{
    public interface IModule<TCommandInfo, TAliasInfo>
        where TAliasInfo : AliasInfo
        where TCommandInfo : CommandInfo<TAliasInfo>
    {
        ICommandContext<TCommandInfo, TAliasInfo> Context { get; }
        Task<Message> ReplyAsync(
            string content,
            ParseMode parseMode = ParseMode.Markdown,
            bool disableWebPagePreview = false,
            bool disableNotification = false,
            int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null
            );
    }
}