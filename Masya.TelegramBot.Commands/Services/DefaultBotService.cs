using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Commands.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Masya.TelegramBot.Commands.Metadata;

namespace Masya.TelegramBot.Commands.Services
{
    public class DefaultBotService : IBotService
    {
        public ITelegramBotClient Client { get; set; }
        public BotServiceOptions Options { get; set; }

        protected readonly IServiceProvider services;

        private readonly ILogger<DefaultBotService> _logger;
        private readonly List<ICollector> _collectors;

        public DefaultBotService(
            IOptions<BotServiceOptions> options,
            IServiceProvider services,
            ILogger<DefaultBotService> logger
        )
        {
            Options = options.Value;
            EnsureTokenExists();
            Client = new TelegramBotClient(Options.Token);
            this.services = services;
            _logger = logger;
            _collectors = new List<ICollector>();
        }

        public DefaultBotService(IServiceProvider services)
        {
            this.services = services;
        }

        public DefaultBotService()
        {
        }

        protected void EnsureTokenExists()
        {
            if (string.IsNullOrEmpty(Options.Token))
            {
                var validationErrors = new List<string>()
                {
                    "Bot token value was null or empty. Please, check your appsettings.json file."
                };
                throw new OptionsValidationException(nameof(Options.Token), typeof(string), validationErrors);
            }
        }

        private async Task HandleMessageAsync(Message message)
        {
            if (!Options.IsEnabled)
            {
                return;
            }

            _logger.LogInformation(string.Format("Received message from: {0}", message.From.ToString()));
            var collector = _collectors.FirstOrDefault(c => c.Chat.Id == message.Chat.Id && c.IsStarted);

            if (collector != null)
            {
                await collector.CollectAsync(message);
                return;
            }

            var commandService = services.GetRequiredService<ICommandService>();
            try
            {
                await commandService.ExecuteCommandAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to execute command: " + ex.Message);
            }
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

        public async Task SetWebhookAsync()
        {
            _logger.LogInformation("Setting up a webhook...");
            await Client.SetWebhookAsync(Options.WebhookHost.Replace("{BOT_TOKEN}", Options.Token));
            _logger.LogInformation("Webhook was set.");
        }

        public async Task HandleUpdateAsync(Update update)
        {
            switch (update.Message)
            {
                case Message message:
                    await HandleMessageAsync(message);
                    break;

                default: return;
            }
        }

        public async Task<BotStatus> GetStatusAsync()
        {
            var me = await Client.GetMeAsync();
            return new BotStatus
            {
                IsWorking = this.Options.IsEnabled,
                Bot = me,
                Host = Options.WebhookHost,
                Token = Options.Token,
            };
        }
    }
}