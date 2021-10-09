using System.Threading.Tasks;
using System.Linq;
using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.DataAccess;
using Microsoft.EntityFrameworkCore;
using Masya.TelegramBot.DatabaseExtensions;

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
        public async Task HandleFavoritesCommand()
        {
            var favorites = await _dbContext.Favorites
                .Include(f => f.RealtyObject)
                .Where(f => f.User.TelegramAccountId == Context.User.Id)
                .Select(f => f.RealtyObject)
                .ToListAsync();

            if (favorites == null || favorites.Count == 0)
            {
                await ReplyAsync(
                    "❌ You have no favorited objects.\nYou can use command /search to search and favorite some objects."
                );
            }

            await SendResultsAsync(favorites);
        }
    }
}