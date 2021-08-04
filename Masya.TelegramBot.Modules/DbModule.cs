using Masya.TelegramBot.Commands;
using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masya.TelegramBot.Modules
{
    public sealed class DbModule : Module
    {
        private readonly ApplicationDbContext _dbContext;

        public DbModule(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Command("/users")]
        [Alias("пользователи", "users")]
        public async Task GetUsersCommandAsync()
        {
            List<User> users = _dbContext.Users.ToList();
            var builder = new StringBuilder();
            foreach(var user in users)
            {
                builder.AppendLine(user.ToString());
            }

            await ReplyAsync(builder.ToString());
        }
    }
}
