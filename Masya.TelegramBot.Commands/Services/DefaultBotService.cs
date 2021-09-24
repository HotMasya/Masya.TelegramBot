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
    public class DefaultBotService<TCommandInfo, TAliasInfo> : IBotService<TCommandInfo, TAliasInfo>
     where TAliasInfo : AliasInfo
     where TCommandInfo : CommandInfo<TAliasInfo>
    {
        public ITelegramBotClient Client { get; set; }
        public BotServiceOptions Options { get; set; }

        protected readonly IServiceProvider services;

        private readonly ILogger<IBotService<TCommandInfo, TAliasInfo>> _logger;
        private readonly List<ICollector<TCommandInfo, TAliasInfo>> _collectors;

        public DefaultBotService(
            IOptions<BotServiceOptions> options,
            IServiceProvider services,
            ILogger<IBotService<TCommandInfo, TAliasInfo>> logger
        )
        {
            Options = options.Value;
            LoadBot();
            this.services = services;
            _logger = logger;
            _collectors = new List<ICollector<TCommandInfo, TAliasInfo>>();
        }

        public DefaultBotService(
            IServiceProvider services,
            ILogger<IBotService<TCommandInfo, TAliasInfo>> logger
        )
        {
            this.services = services;
            _logger = logger;
            _collectors = new List<ICollector<TCommandInfo, TAliasInfo>>();
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

        private void HandleCallback(CallbackQuery callback)
        {
            var commandService = services.GetRequiredService<ICommandService<TCommandInfo, TAliasInfo>>();
            try
            {
                commandService.HandleCallback(callback);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to handle callback: " + ex.ToString());
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

            var commandService = services.GetRequiredService<ICommandService<TCommandInfo, TAliasInfo>>();
            try
            {
                await commandService.ExecuteCommandAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to execute command: " + ex.ToString());
            }
        }

        public ICollector<TCommandInfo, TAliasInfo> CreateMessageCollector(Chat chat, TimeSpan messageTimeout)
        {
            var mcol = new MessageCollector<TCommandInfo, TAliasInfo>(chat, this, messageTimeout);
            mcol.OnFinish += (sender, args) => RemoveCollector(mcol);
            mcol.OnMessageTimeout += (sender, args) => RemoveCollector(mcol);

            _collectors.Add(mcol);
            return mcol;
        }

        private void RemoveCollector(ICollector<TCommandInfo, TAliasInfo> col)
        {
            if (_collectors.Contains(col))
            {
                _collectors.Remove(col);
            }
        }

        public async Task SetWebhookAsync()
        {
            _logger.LogInformation("Setting up a webhook...");
            await Client.SetWebhookAsync(Options.WebhookHost.Replace("{BOT_TOKEN}", Options.Token));
            _logger.LogInformation("Webhook was set.");
        }

        public async Task HandleUpdateAsync(Update update)
        {
            if (update.Message != null)
            {
                await HandleMessageAsync(update.Message);
            }

            if (update.CallbackQuery != null)
            {
                HandleCallback(update.CallbackQuery);
            }
        }

        public async Task<BotStatus> GetSettingsAsync()
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

        public async Task<bool> TestSettingsAsync(string token, string webhookHost)
        {
            Client = new TelegramBotClient(token);
            var me = await Client.GetMeAsync();
            if (me is null)
            {
                return false;
            }

            try
            {
                await Client.SetWebhookAsync(webhookHost);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public virtual void LoadBot()
        {
            EnsureTokenExists();
            Client = new TelegramBotClient(Options.Token);
        }
    }
}