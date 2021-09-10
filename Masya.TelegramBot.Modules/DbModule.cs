using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Models;
using Masya.TelegramBot.DatabaseExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masya.TelegramBot.Modules
{
    public sealed class DbModule : DatabaseModule
    {
        private readonly ApplicationDbContext _dbContext;

        public DbModule(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Command("/users")]
        public async Task GetUsersCommandAsync()
        {
            List<User> users = _dbContext.Users.ToList();
            var builder = new StringBuilder();

            if (users.Count == 0)
            {
                await ReplyAsync("There are no users in the database.");
                return;
            }

            foreach (var user in users)
            {
                builder.AppendLine(user.ToString());
            }

            await ReplyAsync(builder.ToString());
        }
    }
}
