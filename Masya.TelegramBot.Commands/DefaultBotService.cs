using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Commands.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Microsoft.Extensions.Logging;

namespace Masya.TelegramBot.Commands
{
    public sealed class DefaultBotService : IBotService
    {
        public ITelegramBotClient Client { get; }
        public BotServiceOptions Options { get; }

        private readonly IServiceProvider _services;
        private readonly ILogger<DefaultBotService> _logger;

        public DefaultBotService(IOptions<BotServiceOptions> options, IServiceProvider services, ILogger<DefaultBotService> logger)
        {
            Options = options.Value;
            EnsureTokenExists();
            Client = new TelegramBotClient(Options.Token);
            _services = services;
            _logger = logger;
        }

        public DefaultBotService()
        {
        }

        private void EnsureTokenExists()
        {
            if (string.IsNullOrEmpty(Options.Token))
            {
                var validationErrors = new List<string>() { "Bot token value was null or empty. Please, check your appsettings.json file." };
                throw new OptionsValidationException(nameof(Options.Token), typeof(string), validationErrors);
            }
        }

        public async Task Run(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting bot...");
            await Task.Run(() => Client.StartReceiving(new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync), cancellationToken));
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken = default)
        {
            switch (update.Message)
            {
                case Message message:
                    try
                    {
                        var commandService = _services.GetRequiredService<ICommandService>();

                        if (!commandService.TryAddStepMessage(message))
                        {
                            await commandService.ExecuteCommandAsync(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        await HandleErrorAsync(botClient, ex, cancellationToken);
                    }
                    break;

                default:
                    return;
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken = default)
        {
            _logger.LogError(exception.Message);
            return Task.CompletedTask;
        }
    }
}