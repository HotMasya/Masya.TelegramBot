using System.IO;
using System.Threading.Tasks;

namespace Telegram.Bot
{
    public static class TelegramBotClientExtensions
    {
        public static async Task<byte[]> DownloadAvatarAsync(this ITelegramBotClient client, long userId)
        {
            var photos = await client.GetUserProfilePhotosAsync(userId, 0, 1);
            if (photos.Photos == null || photos.Photos.LongLength == 0)
            {
                return null;
            }

            var actualAvatar = photos.Photos[^1][0];
            var fileMeta = await client.GetFileAsync(actualAvatar.FileId);
            using var ms = new MemoryStream();
            await client.DownloadFileAsync(fileMeta.FilePath, ms);
            return ms.ToArray();
        }
    }
}