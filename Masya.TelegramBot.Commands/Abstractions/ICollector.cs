using Masya.TelegramBot.Commands.Events;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands.Abstractions
{
    public delegate void MessageHandler(object sender, CollectorEventArgs message);

    public interface ICollector
    {
        Chat Chat { get; }
        IBotService BotService { get; }
        bool IsStarted { get; }
        void Start();
        void Finish();
        ICollector Collect(Func<Message, object> selector);
        Task CollectAsync(Message message);
        event MessageHandler OnMessageReceived;
        event EventHandler OnMessageTimeout;
        event EventHandler OnStart;
        event EventHandler OnFinish;
    }
}
