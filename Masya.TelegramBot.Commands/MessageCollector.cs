using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Commands.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands
{
    public sealed class MessageCollector: ICollector
    {
        public Chat Chat { get; }
        public IBotService BotService { get; }
        public bool IsStarted { get; private set; }
        public event MessageHandler OnMessageReceived;
        public event EventHandler OnMessageTimeout;
        public event EventHandler OnStart;
        public event EventHandler OnFinish;

        private readonly List<Func<Message, object>> _filters;
        private readonly SemaphoreSlim _semaphore;
        private readonly Queue<Message> _collectedMessages;
        private readonly TimeSpan _messageTimeout;
        private CancellationTokenSource _cts;

        internal MessageCollector(Chat chat, IBotService botService, TimeSpan messageTimeout)
        {
            _filters = new();
            _semaphore = new SemaphoreSlim(1, 1);
            _collectedMessages = new Queue<Message>();
            _cts = new CancellationTokenSource();
            _messageTimeout = messageTimeout;

            IsStarted = false;
            Chat = chat;
            BotService = botService;
        }

        public ICollector Collect(Func<Message, object> selector)
        {
            _filters.Add(selector);
            return this;
        }

        public void Start()
        {
            if(_filters.Count == 0)
            {
                throw new InvalidOperationException("Provide fields to collect first.");
            }

            OnStart?.Invoke(this, EventArgs.Empty);
            _cts = new CancellationTokenSource(_messageTimeout);
            new Task(async () => await RunCollectorLoop())
                .Start();
            IsStarted = true;
        }

        public void Finish()
        {
            IsStarted = false;
            _cts.Cancel();
            OnFinish?.Invoke(this, EventArgs.Empty);
        }

        public async Task CollectAsync(Message message)
        {
            if(IsSuitable(message))
            {
                await _semaphore.WaitAsync();
                _collectedMessages.Enqueue(message);
                _semaphore.Release();
            }
        }

        private bool IsSuitable(Message message)
        {
            foreach(var f in _filters)
            {
                var result = f.Invoke(message);
                if(result != null)
                {
                    return true;
                }
            }
            return false;
        }

        public async Task RunCollectorLoop()
        {
            while(!_cts.IsCancellationRequested)
            {
                if(_collectedMessages.Count > 0)
                {
                    await _semaphore.WaitAsync();
                    PopAllMessages();
                    _cts = new CancellationTokenSource(_messageTimeout);
                    _semaphore.Release();
                }
            }

            if (IsStarted)
            {
                OnMessageTimeout?.Invoke(this, EventArgs.Empty);
            }
        }

        private void PopAllMessages()
        {
            while (_collectedMessages.Count > 0)
            {
                OnMessageReceived?.Invoke(
                    this,
                    new CollectorEventArgs(_collectedMessages.Dequeue())
                    );
            }
        }
    }
}
