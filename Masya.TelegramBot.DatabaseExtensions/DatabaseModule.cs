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
using Masya.TelegramBot.DatabaseExtensions.Types;
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
                    string.Format("🏢 District: *{0}*", obj.District.Value)
                );
            }

            if (obj.Street != null)
            {
                builder.AppendLine(
                    string.Format("🏢 Address: *{0}*", obj.Street.Value)
                );
            }

            if (obj.State != null)
            {
                builder.AppendLine(
                    string.Format("🔨 State: *{0}*", obj.State.Value)
                );
            }

            if (obj.WallMaterial != null)
            {
                builder.AppendLine(
                    string.Format("🧱 Walls material: *{0}*", obj.WallMaterial.Value)
                );
            }

            if (obj.Rooms.HasValue)
            {
                builder.AppendLine(
                    string.Format("🚪 Rooms: *{0}*", obj.Rooms.Value)
                );
            }

            if (obj.Floor.HasValue)
            {
                builder.AppendLine(
                    string.Format("🏦 Floor: *{0}*", obj.Floor.Value)
                );
            }

            if (obj.TotalFloors.HasValue)
            {
                builder.AppendLine(
                    string.Format("🏦 Total floors: *{0}*", obj.TotalFloors.Value)
                );
            }

            if (obj.TotalArea.HasValue)
            {
                builder.AppendLine(
                    string.Format("🌏 Total Area: *{0}*", obj.TotalArea.Value)
                );
            }

            if (obj.LivingSpace.HasValue)
            {
                builder.AppendLine(
                    string.Format("🏚 Living Area: *{0}*", obj.LivingSpace.Value)
                );
            }

            if (obj.KitchenSpace.HasValue)
            {
                builder.AppendLine(
                    string.Format("🍽 Kitchen Area: *{0}*", obj.KitchenSpace.Value)
                );
            }

            if (obj.LotArea.HasValue)
            {
                builder.AppendLine(
                    string.Format("🏚 Lot Area: *{0}*", obj.LotArea.Value)
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
            if (favorites == null)
            {
                return null;
            }

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

        protected async Task SendObjectsAsync(List<RealtyObject> results, List<RealtyObject> favorites = null, int delay = 5)
        {
            using var httpClient = new HttpClient();

            foreach (var r in results)
            {
                if (r.Images != null)
                {
                    var photos = new List<InputMediaPhoto>();
                    foreach (var image in r.Images)
                    {
                        photos.Add(
                            await UrlToTelegramPhotoAsync(
                                image.Url,
                                image.Id.ToString(),
                                httpClient
                            )
                        );
                    }

                    if (photos.Count > 0)
                    {
                        await Context.BotService.Client.SendMediaGroupAsync(
                            chatId: Context.Chat.Id,
                            media: photos.Take(10),
                            disableNotification: true
                        );
                    }
                }

                await ReplyAsync(
                    content: BuildRealtyObjectDescr(r),
                    parseMode: ParseMode.Markdown,
                    replyMarkup: GenerateFavoriteButton(r, favorites),
                    disableNotification: true
                );
                await Task.Delay(TimeSpan.FromSeconds(delay));
            }
        }

        private static async Task<InputMediaPhoto> UrlToTelegramPhotoAsync(
            string url,
            string fileName,
            HttpClient client
        )
        {
            try
            {
                var fImageBytes = await client.GetByteArrayAsync(url);
                var inputFile = new InputMedia(new MemoryStream(fImageBytes), fileName);
                return new InputMediaPhoto(inputFile);
            }
            catch
            {
                return null;
            }
        }
    }
}