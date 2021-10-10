using System;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Commands.Metadata;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Masya.TelegramBot.Commands
{
    public abstract class Module<TCommandInfo, TAliasInfo> : IModule<TCommandInfo, TAliasInfo>
        where TAliasInfo : AliasInfo
        where TCommandInfo : CommandInfo<TAliasInfo>
    {
        public ICommandContext<TCommandInfo, TAliasInfo> Context { get; init; }

        public Task<Message> EditMessageAsync(
            string text = null,
            ParseMode? parseMode = null,
            InlineKeyboardMarkup replyMarkup = null
        )
        {
            if (!string.IsNullOrEmpty(text))
            {
                return Context.BotService.Client.EditMessageTextAsync(
                    chatId: Context.Chat.Id,
                    messageId: Context.Message.MessageId,
                    text: text,
                    parseMode: parseMode,
                    replyMarkup: replyMarkup,
                    cancellationToken: default
                    );
            }
            else if (replyMarkup != null)
            {
                return Context.BotService.Client.EditMessageReplyMarkupAsync(
                    chatId: Context.Chat.Id,
                    messageId: Context.Message.MessageId,
                    replyMarkup: replyMarkup
                );
            }

            return Context.BotService.Client.EditMessageReplyMarkupAsync(
                    chatId: Context.Chat.Id,
                    messageId: Context.Message.MessageId,
                    replyMarkup: null
            );
        }

        public Task<Message> ReplyAsync(
            string content,
            ParseMode? parseMode = null,
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
                chatId: Context.Chat.Id,
                text: content,
                parseMode: parseMode,
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