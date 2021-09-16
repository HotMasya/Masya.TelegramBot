using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Modules;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DatabaseExtensions.Metadata;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Masya.TelegramBot.Api
{
    public static class Program
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
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder
                    .AddEnvironmentVariables()
                    .AddJsonFile(
                      path: Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"),
                      optional: false,
                      reloadOnChange: true
                    );
                })
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.AddSerilog();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
