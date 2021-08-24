using System;
using System.Linq;
using Masya.TelegramBot.Commands.Options;
using Masya.TelegramBot.Commands.Services;
using Masya.TelegramBot.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace Masya.TelegramBot.DatabaseExtensions
{
    public class DatabaseBotService : DefaultBotService
    {
        public DatabaseBotService(IServiceProvider services) : base(services)
        {
            LoadBot();
        }

        public void LoadBot()
        {
            Options = GetOptionsFromDb();
            EnsureTokenExists();
            Client = new TelegramBotClient(Options.Token);
        }

        public BotServiceOptions GetOptionsFromDb()
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var dbOptions = dbContext.BotSettings.FirstOrDefault();
            if (dbOptions is null)
            {
                throw new InvalidOperationException("Unable to load bot options from database. The bot options table was empty.");
            }

            return new BotServiceOptions
            {
                Token = dbOptions.BotToken,
                WebhookHost = dbOptions.WebhookHost,
                IsEnabled = dbOptions.IsEnabled
            };
        }
    }
}