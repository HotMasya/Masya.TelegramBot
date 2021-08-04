using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Commands.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Masya.TelegramBot.Commands.Services
{
    public sealed class DefaultBotService : IBotService
    {
        public ITelegramBotClient Client { get; }
        public BotServiceOptions Options { get; }

        private readonly IServiceProvider _services;
        private readonly ILogger<DefaultBotService> _logger;
        private readonly List<ICollector> _collectors;

        public DefaultBotService(IOptions<BotServiceOptions> options, IServiceProvider services, ILogger<DefaultBotService> logger)
        {
            Options = options.Value;
            EnsureTokenExists();
            Client = new TelegramBotClient(Options.Token);
            _services = services;
            _logger = logger;
            _collectors = new List<ICollector>();
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

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting bot...");
            await Task.Run(
                () => Client.StartReceiving( new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),cancellationToken),
                cancellationToken
                );
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken = default)
        {
            switch (update.Message)
            {
                case Message message:
                    try
                    {
                        await HandleMessageAsync(message);
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

        private async Task HandleMessageAsync(Message message)
        {
            _logger.LogInformation(string.Format("Received message from: {0}", message.From.ToString()));
            var collector = _collectors.FirstOrDefault(c => c.Chat.Id == message.Chat.Id && c.IsStarted);

            if(collector != null)
            {
                await collector.CollectAsync(message);
                return;
            }

            var commandService = _services.GetRequiredService<ICommandService>();
            await commandService.ExecuteCommandAsync(message);
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken = default)
        {
            _logger.LogError(exception.Message);
            return Task.CompletedTask;
        }

        public ICollector CreateMessageCollector(Chat chat, TimeSpan messageTimeout)
        {  
            var mcol = new MessageCollector(chat, this, messageTimeout);
            mcol.OnFinish += (sender, args) => RemoveCollector(mcol);
            mcol.OnMessageTimeout += (sender, args) => RemoveCollector(mcol);

            _collectors.Add(mcol);
            return mcol;
        }

        private Task RemoveCollector(ICollector col)
        {
            if (_collectors.Contains(col))
            {
                _collectors.Remove(col);
            }
            return Task.CompletedTask;
        }
    }
}