using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Modules;

namespace Masya.TelegramBot.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        public static async Task StartRequiredServices(IServiceProvider services)
        {
            var commandService = services.GetRequiredService<ICommandService>();
            var botService = services.GetRequiredService<IBotService>();
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
