using System.Threading.Tasks;
using System.Linq;
using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.DataAccess;
using Microsoft.EntityFrameworkCore;
using Masya.TelegramBot.DatabaseExtensions;
using Masya.TelegramBot.DataAccess.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace Masya.TelegramBot.Modules
{
    public sealed class FavoritesModule : DatabaseModule
    {
        private readonly ApplicationDbContext _dbContext;

        public FavoritesModule(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Command("/favorites")]
        public async Task HandleFavoritesCommandAsync()
        {
            var favorites = await _dbContext.Favorites
                .AsNoTracking()
                .Include(f => f.RealtyObject)
                .Where(f => f.User.TelegramAccountId == Context.User.Id)
                .Select(f => f.RealtyObject)
                .ToListAsync();

            if (favorites == null || favorites.Count == 0)
            {
                await ReplyAsync(
                    "❌ You have no favorited objects.\n\nUse command /search to *search and favorite* some objects."
                );
            }

            await SendObjectsAsync(favorites, favorites, 3);
        }

        [Callback(CallbackDataTypes.RemoveFromFavorites)]
        public async Task HandleRemoveFromFavoritesAsync(int objId)
        {
            var favorite = await _dbContext.Favorites.FirstOrDefaultAsync(f => f.RealtyObjectId == objId);

            if (favorite != null)
            {
                _dbContext.Favorites.Remove(favorite);
                await _dbContext.SaveChangesAsync();

                await EditMessageAsync(
                    replyMarkup: new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithCallbackData(
                            "❤ Favorite",
                            string.Join(
                                Context.CommandService.Options.CallbackDataSeparator,
                                CallbackDataTypes.AddToFavorites,
                                objId.ToString()
                            )
                        )
                    )
                );
            }
        }

        [Callback(CallbackDataTypes.AddToFavorites)]
        public async Task HandleAddToFavoritesAsync(int objId)
        {
            var obj = await _dbContext.RealtyObjects.FirstOrDefaultAsync(ro => ro.Id == objId);
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.TelegramAccountId == Context.User.Id);
            if (obj != null && user != null)
            {
                _dbContext.Favorites.Add(
                    new Favorites
                    {
                        User = user,
                        RealtyObject = obj
                    }
                );
                await _dbContext.SaveChangesAsync();
                await EditMessageAsync(
                    replyMarkup: new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithCallbackData(
                            "❌ Remove from favorites",
                            string.Join(
                                Context.CommandService.Options.CallbackDataSeparator,
                                CallbackDataTypes.RemoveFromFavorites,
                                objId.ToString()
                            )
                        )
                    )
                );
            }
        }
    }
}