using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Modules;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DatabaseExtensions.Metadata;

namespace Masya.TelegramBot.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await StartRequiredServices(host.Services);
            await host.RunAsync();
        }

        public static async Task StartRequiredServices(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await ApplicationDbContext.SeedDatabase(dbContext);
            var commandService = services.GetRequiredService<ICommandService<DatabaseCommandInfo, DatabaseAliasInfo>>();
            var botService = services.GetRequiredService<IBotService<DatabaseCommandInfo, DatabaseAliasInfo>>();
            await commandService.LoadCommandsAsync(typeof(BasicModule).Assembly);
            await botService.SetWebhookAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
