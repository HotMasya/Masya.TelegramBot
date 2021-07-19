using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Commands.Options;
using Masya.TelegramBot.Commands.Attributes;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands
{
    public sealed class DefaultCommandService : ICommandService
    {
        public IBotService BotService { get; }
        public CommandServiceOptions Options { get; }

        private readonly List<MethodInfo> _commandMethods;
        private readonly IServiceProvider _services;
        private readonly ILogger<DefaultCommandService> _logger;
        private readonly List<Chat> _activeStepChats;
        private readonly List<Message> _stepMessages;
        private object syncMessages, syncChats;

        public DefaultCommandService(IOptionsMonitor<CommandServiceOptions> options, IBotService botService, IServiceProvider services, ILogger<DefaultCommandService> logger)
        {
            BotService = botService;
            Options = options.CurrentValue;
            _commandMethods = new List<MethodInfo>();
            _activeStepChats = new List<Chat>();
            _stepMessages = new List<Message>();
            _services = services;
            _logger = logger;

            syncMessages = new object();
            syncChats = new object();
        }

        public bool TryAddStepMessage(Message message)
        {
            if (_activeStepChats.Any(c => c.Id == message.Chat.Id))
            {
                lock (syncMessages)
                {
                    _stepMessages.Add(message);
                }
                return true;
            }
            return false;
        }

        public async Task ExecuteCommandAsync(Message message)
        {
            await Task.Run(() =>
            {
                if (!IsCommand(message?.Text))
                {
                    return;
                }

                CommandParts parts = new CommandParts(message?.Text, Options);
                MethodInfo method = _commandMethods.FirstOrDefault(cm => CommandFilter(cm, parts.Name));

                if (method == null)
                {
                    return;
                }

                if (parts.ArgsStr.Length == 0 && method.GetParameters().Length != 0)
                {
                    Task t = new Task(async () => await ExecuteCommandByStepsAsync(message, method, parts));
                    t.Start();
                }
                else
                {
                    var moduleInstance = ActivatorUtilities.CreateInstance(_services, method.DeclaringType, new object[] { });
                    var propInfo = method.DeclaringType.GetProperty("Context");
                    var context = new DefaultCommandContext(BotService, message.Chat, message.From, message);
                    propInfo.SetValue(moduleInstance, context);
                    method.Invoke(moduleInstance, parts.MatchParamTypes(method));
                }
            });
        }

        public async Task LoadModulesAsync(Assembly assembly)
        {
            _logger.LogInformation("Loading commands from modules from assembly: " + assembly.GetName().Name);
            int count = await Task<int>.Run(() =>
            {
                foreach (var type in assembly.DefinedTypes)
                {
                    if (IsModuleType(type))
                    {
                        foreach (var method in type.DeclaredMethods)
                        {
                            if (IsValidCommand(method))
                            {
                                _commandMethods.Add(method);
                            }
                        }
                    }
                }

                return _commandMethods.Count;
            });
            _logger.LogInformation($"Loaded {count} commands from assembly: " + assembly.GetName().Name);
        }

        private bool IsCommand(string content)
        {
            if (!string.IsNullOrEmpty(content) && !content.StartsWith(Options.Prefix))
            {
                return false;
            }

            if (content.Equals(Options.Prefix.ToString()))
            {
                return false;
            }

            return true;
        }

        private bool CommandFilter(MethodInfo info, string commandName)
        {
            CommandAttribute cmdAttr = info.GetCustomAttribute<CommandAttribute>();
            AliasAttribute aliasAttr = info.GetCustomAttribute<AliasAttribute>();
            return (cmdAttr != null && cmdAttr.Name.Equals(commandName)) ||
                (aliasAttr != null && aliasAttr.Aliases.Any(a => a.Equals(commandName)));
        }

        private bool IsValidCommand(MethodInfo method)
        {
            return method.GetCustomAttribute<CommandAttribute>() != null &&
                method.IsPublic &&
                !method.IsAbstract &&
                !method.IsGenericMethod &&
                (method.ReturnType == typeof(Task) || method.ReturnType == typeof(Task<>));
        }

        private bool IsModuleType(TypeInfo type)
        {
            return type.IsPublic &&
                !type.IsAbstract &&
                !type.IsGenericType &&
                type.BaseType.Equals(typeof(Module));
        }

        private async Task ExecuteCommandByStepsAsync(Message message, MethodInfo method, CommandParts parts, CancellationToken cancellationToken = default)
        {
            var moduleInstance = ActivatorUtilities.CreateInstance(_services, method.DeclaringType, new object[] { });
            var propInfo = method.DeclaringType.GetProperty("Context");
            var context = new DefaultCommandContext(BotService, message.Chat, message.From, message);
            propInfo.SetValue(moduleInstance, context);

            lock (syncChats)
            {
                _activeStepChats.Add(message.Chat);
            }

            List<object> result = new List<object>();
            ParameterInfo[] parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length;)
            {
                if (cancellationToken.IsCancellationRequested) return;
                var nameAttr = parameters[i].GetCustomAttribute<ParamNameAttribute>();
                string paramName = nameAttr?.Name ?? parameters[i].Name;

                await BotService.Client.SendTextMessageAsync(message.Chat, $"Пожалуйста, укажите {paramName}");

                CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(Options.StepCommandTimeout));
                string response = await GetAnswerAsync(message.Chat, cts.Token);

                if(response == null)
                {
                    await BotService.Client.SendTextMessageAsync(message.Chat, "Время ожидания истекло. Введите команду ещё раз.");
                    lock (syncChats)
                    {
                        _activeStepChats.Remove(message.Chat);
                    }
                    return;
                }

                try
                {
                    result.Add(parts.MatchTypeParam(parameters[i], response, result.Count));
                    i++;
                }
                catch
                {
                    continue;
                }
            }

            lock (syncChats)
            {
                _activeStepChats.Remove(message.Chat);
            }

            if (cancellationToken.IsCancellationRequested) return;
            method.Invoke(moduleInstance, result.ToArray());
        }

        private Task<string> GetAnswerAsync(Chat chat, CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                lock (syncMessages)
                {
                    var receivedMessage = _stepMessages.FirstOrDefault(msg => msg.Chat.Id == chat.Id);
                    if (receivedMessage != null)
                    {
                        _stepMessages.Remove(receivedMessage);
                        return Task.FromResult(receivedMessage?.Text);
                    }
                }
            }
            return Task.FromResult<string>(null);
        }

    }
}