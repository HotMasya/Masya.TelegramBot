using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.IO;
using Telegram.Bot;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Commands;
using Masya.TelegramBot.Commands.Options;
using System.Threading;
using Masya.TelegramBot.Modules;
using Masya.RPNCalculator.Core;
using Masya.RPNCalculator.Core.Abstractions;

namespace Masya.TelegramBot.ConsoleUI
{
    class Program
    {
        private static IConfiguration _configuration;

        public static async Task Main()
        {
            _configuration = CreateConfigurationBuilder().Build();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            IHost host = CreateHostBuilder().Build();
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken cancellationToken = cts.Token;

            var botService = host.Services.GetService<IBotService>();
            var commandService = host.Services.GetService<ICommandService>();

            await commandService.LoadModulesAsync(typeof(TestModule).Assembly);
            await botService.Run(cancellationToken);
            await host.RunAsync(cancellationToken);
        }

        public static IConfigurationBuilder CreateConfigurationBuilder() =>
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile("appsettings.Development.json", true, true)
                .AddEnvironmentVariables();

        public static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services
                    .Configure<BotServiceOptions>(_configuration.GetSection("Bot"))
                    .Configure<CommandServiceOptions>(_configuration.GetSection("Commands"))
                    .AddSingleton<ICalculatorFactory, DefaultCalculatorFactory>()
                    .AddSingleton<TelegramBotClient>()
                    .AddSingleton<IBotService, DefaultBotService>()
                    .AddSingleton<ICommandService, DefaultCommandService>();
                })
                .UseSerilog();
    }
}
