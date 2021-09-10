using Masya.TelegramBot.Commands.Events;
using Masya.TelegramBot.Commands.Metadata;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands.Abstractions
{
    public delegate void MessageHandler(object sender, CollectorEventArgs message);

    public interface ICollector<TCommandInfo, TAliasInfo>
        where TAliasInfo : AliasInfo
        where TCommandInfo : CommandInfo<TAliasInfo>
    {
        Chat Chat { get; }
        IBotService<TCommandInfo, TAliasInfo> BotService { get; }
        bool IsStarted { get; }
        void Start();
        void Finish();
        ICollector<TCommandInfo, TAliasInfo> Collect(Func<Message, object> selector);
        Task CollectAsync(Message message);
        event MessageHandler OnMessageReceived;
        event EventHandler OnMessageTimeout;
        event EventHandler OnStart;
        event EventHandler OnFinish;
    }
}
