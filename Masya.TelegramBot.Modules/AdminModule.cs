using System.Linq;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Models;
using Masya.TelegramBot.DatabaseExtensions;
using Telegram.Bot.Types.Enums;

namespace Masya.TelegramBot.Modules
{
    public sealed class AdminModule : DatabaseModule
    {
        private readonly ApplicationDbContext _dbContext;
        private const long BotDeveloperId = 619660711;

        public AdminModule(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Command("/reset")]
        public async Task HandleResetAsync()
        {
            if (Context.User.Id != BotDeveloperId)
            {
                return;
            }

            var developer = _dbContext.Users.FirstOrDefault(u => u.TelegramAccountId == Context.User.Id);

            if (developer != null)
            {
                _dbContext.Users.Remove(developer);
                await _dbContext.SaveChangesAsync();
                await ReplyAsync("✅ Sucessfully removed you from the database!");
                return;
            }

            await ReplyAsync("⚠ You are not in the database yet.");
        }

        [Command("/dev")]
        public async Task HandleSuperUserAsync()
        {
            if (Context.User.Id != BotDeveloperId)
            {
                return;
            }

            var developer = _dbContext.Users.FirstOrDefault(u => u.TelegramAccountId == Context.User.Id);

            if (developer != null)
            {
                developer.Permission = Permission.SuperAdmin;
                await _dbContext.SaveChangesAsync();
                await ReplyAsync("✅ Sucessfully granted *Super Admin* permission to you!", ParseMode.Markdown);
                return;
            }

            await ReplyAsync("⚠ You are not in the database yet.");
        }
    }
}
