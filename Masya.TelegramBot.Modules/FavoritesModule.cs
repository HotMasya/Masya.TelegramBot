using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.DataAccess;

namespace Masya.TelegramBot.Modules
{
    public sealed class FavoritesModule
    {
        private readonly ApplicationDbContext _dbContext;

        public FavoritesModule(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Command("/favorites")]
        public async Task HandleFavoritesCommand()
        {

        }
    }
}