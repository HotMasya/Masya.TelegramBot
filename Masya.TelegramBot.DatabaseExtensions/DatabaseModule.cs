using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands;
using Masya.TelegramBot.DataAccess.Models;
using Masya.TelegramBot.DatabaseExtensions.Metadata;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Masya.TelegramBot.DatabaseExtensions
{
    public class DatabaseModule : Module<DatabaseCommandInfo, DatabaseAliasInfo>
    {
        private static string BuildRealtyObjectDescr(RealtyObject obj)
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(obj.Description))
            {
                builder.AppendLine(obj.Description);
            }

            if (obj.District != null)
            {
                builder.AppendLine(
                    string.Format("\n🏢 District: *{0}*", obj.District.Value)
                );
            }

            if (obj.Street != null)
            {
                builder.AppendLine(
                    string.Format("\n🏢 Address: *{0}*", obj.Street.Value)
                );
            }

            if (obj.State != null)
            {
                builder.AppendLine(
                    string.Format("\n🔨 State: *{0}*", obj.State.Value)
                );
            }

            if (obj.WallMaterial != null)
            {
                builder.AppendLine(
                    string.Format("\n🧱 State: *{0}*", obj.WallMaterial.Value)
                );
            }

            if (obj.Rooms.HasValue)
            {
                builder.AppendLine(
                    string.Format("\n🚪 Rooms: *{0}*", obj.Rooms.Value)
                );
            }

            if (obj.Floor.HasValue)
            {
                builder.AppendLine(
                    string.Format("\n🏦 Floor: *{0}*", obj.Floor.Value)
                );
            }

            if (obj.TotalFloors.HasValue)
            {
                builder.AppendLine(
                    string.Format("\n🏦 Total floors: *{0}*", obj.TotalFloors.Value)
                );
            }

            if (!string.IsNullOrEmpty(obj.Phone))
            {
                builder.AppendLine(
                    string.Format("\n📞 Contact(s): *{0}*", obj.Phone)
                );
            }

            return builder.ToString();
        }

        private InlineKeyboardMarkup GenerateFavoriteButton(RealtyObject obj, List<RealtyObject> favorites)
        {
            if (favorites.Any(f => f.Id == obj.Id))
            {
                return new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData(
                        "❌ Remove from favorites",
                        string.Join(
                            Context.CommandService.Options.CallbackDataSeparator,
                            CallbackDataTypes.RemoveFromFavorites,
                            obj.Id.ToString()
                        )
                    )
                );
            }

            return new InlineKeyboardMarkup(
                InlineKeyboardButton.WithCallbackData(
                    "❤ Favorite",
                    string.Join(
                        Context.CommandService.Options.CallbackDataSeparator,
                        CallbackDataTypes.AddToFavorites,
                        obj.Id.ToString()
                    )
                )
            );
        }

        protected async Task SendResultsAsync(List<RealtyObject> results, List<RealtyObject> favorites)
        {
            using var httpClient = new HttpClient();

            foreach (var r in results)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                if (r.Images != null && r.Images.Count > 0)
                {
                    var photos = new List<InputMediaPhoto>
                    {
                        await UrlToTelegramPhotoAsync(
                            r.Images[0].Url,
                            r.Images[0].Id.ToString(),
                            httpClient,
                            BuildRealtyObjectDescr(r)
                        )
                    };

                    for (int i = 1; i < r.Images.Count; i++)
                    {
                        var photo = await UrlToTelegramPhotoAsync(
                                r.Images[i].Url,
                                r.Images[i].Id.ToString(),
                                httpClient
                        );

                        if (photo == null) continue;

                        photos.Add(photo);
                    }

                    if (photos.Count > 0)
                    {
                        var messages = await Context.BotService.Client.SendMediaGroupAsync(
                            chatId: Context.Chat.Id,
                            media: photos.Take(10),
                            disableNotification: true
                        );

                        if (messages.Length > 0)
                        {
                            await Context.BotService.Client.EditMessageReplyMarkupAsync(
                                chatId: Context.Chat.Id,
                                messageId: messages[0].MessageId,
                                replyMarkup: GenerateFavoriteButton(r, favorites)
                            );
                        }
                        continue;
                    }
                }

                await ReplyAsync(
                    content: BuildRealtyObjectDescr(r),
                    parseMode: ParseMode.Markdown,
                    replyMarkup: GenerateFavoriteButton(r, favorites)
                );
            }
        }

        private static async Task<InputMediaPhoto> UrlToTelegramPhotoAsync(
            string url,
            string fileName,
            HttpClient client,
            string caption = null
        )
        {
            try
            {
                var fImageBytes = await client.GetByteArrayAsync(url);
                var inputFile = new InputMedia(new MemoryStream(fImageBytes), fileName);
                var inputPhoto = new InputMediaPhoto(inputFile);
                if (!string.IsNullOrEmpty(caption))
                {
                    inputPhoto.Caption = caption;
                    inputPhoto.ParseMode = ParseMode.Markdown;
                }

                return inputPhoto;
            }
            catch
            {
                return null;
            }
        }
    }
}