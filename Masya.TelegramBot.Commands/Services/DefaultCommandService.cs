using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Masya.TelegramBot.Commands.Abstractions;
using Masya.TelegramBot.Commands.Options;
using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.Commands.Metadata;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Masya.TelegramBot.Commands.Services
{
    public class DefaultCommandService<TCommandInfo, TAliasInfo> : ICommandService<TCommandInfo, TAliasInfo>
        where TAliasInfo : AliasInfo
        where TCommandInfo : CommandInfo<TAliasInfo>, new()
    {
        protected ContactHandlerInfo contactHandler;
        protected readonly List<TCommandInfo> commands;
        protected readonly IServiceProvider services;

        protected readonly ILogger<ICommandService<TCommandInfo, TAliasInfo>> logger;

        public IBotService<TCommandInfo, TAliasInfo> BotService { get; }
        public CommandServiceOptions Options { get; }
        public List<TCommandInfo> Commands => commands;

        public DefaultCommandService(
            IOptionsMonitor<CommandServiceOptions> options,
            IBotService<TCommandInfo, TAliasInfo> botService,
            IServiceProvider services,
            ILogger<ICommandService<TCommandInfo, TAliasInfo>> logger
        )
        {
            BotService = botService;
            Options = options.CurrentValue;
            commands = new List<TCommandInfo>();
            this.services = services;
            this.logger = logger;
        }

        public virtual bool CheckCommandCondition(TCommandInfo commandInfo, Message message)
        {
            return commandInfo is not null
                && commandInfo.MethodInfo is not null
                && commandInfo.IsEnabled;
        }

        protected virtual TCommandInfo GetCommand(string name, Message message)
        {
            return commands.FirstOrDefault(cm => CommandFilter(cm, name));
        }

        public virtual async Task ExecuteCommandAsync(Message message)
        {
            if (message.Contact != null)
            {
                await HandleContact(message);
                return;
            }

            if (string.IsNullOrEmpty(message.Text))
            {
                return;
            }

            await Task.Run(() =>
            {
                if (commands is null || commands.Count == 0)
                {
                    return;
                }

                var parts = new CommandParts(message.Text, Options);
                var commandInfo = GetCommand(parts.Name, message);

                if (!CheckCommandCondition(commandInfo, message))
                {
                    return;
                }

                logger.LogInformation($"Executing command: {parts.Name}");

                if (parts.ArgsStr.Length == 0 && commandInfo.MethodInfo.GetParameters().Length != 0)
                {
                    var t = new Task(async () => await ExecuteCommandByStepsAsync(message, commandInfo.MethodInfo, parts));
                    t.Start();
                }
                else
                {
                    using var scope = services.CreateScope();
                    var moduleInstance = ActivatorUtilities.CreateInstance(
                        scope.ServiceProvider,
                        commandInfo.MethodInfo.DeclaringType,
                        Array.Empty<object>()
                    );
                    var propInfo = commandInfo.MethodInfo.DeclaringType.GetProperty("Context");
                    var context = new DefaultCommandContext<TCommandInfo, TAliasInfo>(BotService, this, message.Chat, message.From, message);
                    propInfo.SetValue(moduleInstance, context);
                    commandInfo.MethodInfo.Invoke(moduleInstance, parts.MatchParamTypes(commandInfo.MethodInfo));
                }
            });
        }

        public virtual async Task LoadCommandsAsync(Assembly assembly)
        {
            commands.Clear();
            logger.LogInformation("Loading commands from modules from assembly: " + assembly.GetName().Name);
            await Task.Run(() =>
            {
                foreach (var type in assembly.DefinedTypes)
                {
                    if (IsModuleType(type))
                    {
                        foreach (var method in type.DeclaredMethods)
                        {
                            if (IsValidCommand(method))
                            {
                                commands.Add(BuildCommandInfo(method));
                            }
                            else if (IsContactHandler(method))
                            {
                                contactHandler = new ContactHandlerInfo(method);
                            }
                        }
                    }
                }
            });
            logger.LogInformation($"Loaded {commands.Count} commands from assembly: " + assembly.GetName().Name);
        }

        public static TCommandInfo BuildCommandInfo(MethodInfo methodInfo)
        {
            return new TCommandInfo
            {
                Name = methodInfo.GetCustomAttribute<CommandAttribute>()?.Name,
                Description = methodInfo.GetCustomAttribute<DescriptionAttribute>()?.Description,
                MethodInfo = methodInfo
            };
        }

        public static bool IsValidCommand(MethodInfo method)
        {
            var commandAttr = method.GetCustomAttribute<CommandAttribute>();
            var regUserAttr = method.GetCustomAttribute<RegisterUserAttribute>();

            if (commandAttr != null && regUserAttr != null)
            {
                throw new Exception("RegisterUserAttribute should not be used with CommandAttribute.");
            }

            return commandAttr != null
                && method.IsPublic
                && !method.IsAbstract
                && !method.IsGenericMethod
                && (method.ReturnType == typeof(Task) || method.ReturnType == typeof(Task<>));
        }

        public static bool IsContactHandler(MethodInfo method)
        {
            return method.GetCustomAttribute<RegisterUserAttribute>() != null
                && method.IsPublic
                && !method.IsAbstract
                && !method.IsGenericMethod
                && (method.ReturnType == typeof(Task) || method.ReturnType == typeof(Task<>));
        }

        public static bool IsModuleType(TypeInfo type)
        {
            return type.IsPublic
                && !type.IsAbstract
                && !type.IsGenericType
                && type.BaseType.Equals(typeof(Module<TCommandInfo, TAliasInfo>));
        }

        private bool CommandFilter(TCommandInfo info, string commandName)
        {
            return info.Name.Equals(commandName)
                || info.Aliases.Any(a => a.Name.Equals(commandName) && a.IsEnabled);
        }

        protected virtual Task ExecuteCommandByStepsAsync(
            Message message,
            MethodInfo method,
            CommandParts parts,
            CancellationToken cancellationToken = default
        )
        {
            using var scope = services.CreateScope();
            var moduleInstance = ActivatorUtilities.CreateInstance(
                scope.ServiceProvider,
                method.DeclaringType,
                Array.Empty<object>()
            );
            var propInfo = method.DeclaringType.GetProperty("Context");
            var context = new DefaultCommandContext<TCommandInfo, TAliasInfo>(BotService, this, message.Chat, message.From, message);
            propInfo.SetValue(moduleInstance, context);

            var result = new List<object>();

            // Parameters index
            int i = 0;
            ParameterInfo[] parameters = method.GetParameters();

            var messageCollector = BotService.CreateMessageCollector(
                message.Chat,
                TimeSpan.FromSeconds(Options.StepCommandTimeout)
            );
            messageCollector.Collect(m => m.Text);
            messageCollector.OnStart += (sender, args) =>
            {
                SendParamMessage(parameters, i, message.Chat.Id, cancellationToken).Wait();
            };

            messageCollector.OnMessageTimeout += (sender, args) =>
            {
                BotService.Client
                .SendTextMessageAsync(
                    message.Chat,
                    "Время ожидания истекло. Введите команду ещё раз.",
                    cancellationToken: cancellationToken
                    )
                .Wait();
            };

            messageCollector.OnMessageReceived += (sender, e) =>
            {
                try
                {
                    result.Add(parts.MatchTypeParam(parameters[i], e.Message.Text));
                    i++;
                    if (i < parameters.Length)
                    {
                        SendParamMessage(parameters, i, e.Message.Chat.Id, cancellationToken).Wait();
                        return;
                    }
                    messageCollector.Finish();
                }
                catch
                {
                    SendParamMessage(parameters, i, e.Message.Chat.Id, cancellationToken).Wait();
                }
            };

            messageCollector.OnFinish += (sender, args) =>
            {
                method.Invoke(moduleInstance, result.ToArray());
            };

            messageCollector.Start();
            return Task.CompletedTask;
        }

        protected virtual async Task SendParamMessage(
            ParameterInfo[] parameters,
            int index,
            long chatId,
            CancellationToken cancellationToken
        )
        {
            var nameAttr = parameters[index].GetCustomAttribute<ParamNameAttribute>();
            string paramName = nameAttr?.Name ?? parameters[index].Name;
            await BotService.Client.SendTextMessageAsync(
                chatId,
                $"Пожалуйста, укажите {paramName}",
                cancellationToken: cancellationToken
                );
        }

        protected virtual Task HandleContact(Message message)
        {
            logger.LogInformation(
                "Received a contact: {0} {1} {2}",
                message.Contact.FirstName,
                message.Contact.LastName,
                message.Contact.PhoneNumber
            );

            if (contactHandler == null) return Task.CompletedTask;

            using var scope = services.CreateScope();
            var moduleInstance = ActivatorUtilities.CreateInstance(
                scope.ServiceProvider,
                contactHandler.MethodInfo.DeclaringType,
                Array.Empty<object>()
            );
            var propInfo = contactHandler.MethodInfo.DeclaringType.GetProperty("Context");
            var context = new DefaultCommandContext<TCommandInfo, TAliasInfo>(BotService, this, message.Chat, message.From, message);
            propInfo.SetValue(moduleInstance, context);

            contactHandler.MethodInfo.Invoke(moduleInstance, new[] { message.Contact });
            return Task.CompletedTask;
        }

        private static bool IsRegisterUserMethod(MethodInfo method)
        {
            var parameters = method.GetParameters();
            return method.GetCustomAttribute<RegisterUserAttribute>() != null &&
                parameters.Length == 1 && parameters[0].ParameterType == typeof(Contact);
        }
    }
}