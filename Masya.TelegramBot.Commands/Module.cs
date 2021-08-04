using System;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Abstractions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Masya.TelegramBot.Commands
{
    public abstract class Module : IModule
    {
        public ICommandContext Context { get; init; }

        public Task<Message> ReplyAsync(
            string content,
            ParseMode parseMode = ParseMode.Html,
            bool disableWebPagePreview = false,
            bool disableNotification = false,
            int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null
            )
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new InvalidOperationException("Message content was null or empty.");
            }

            return Context?.BotService.Client.SendTextMessageAsync(
                chatId: Context?.Chat?.Id,
                text: content,
                parseMode: parseMode,
                entities: null,
                disableWebPagePreview: disableWebPagePreview,
                disableNotification: disableNotification,
                replyToMessageId: replyToMessageId,
                allowSendingWithoutReply: false,
                replyMarkup: replyMarkup,
                cancellationToken: default
                );
        }
    }
}