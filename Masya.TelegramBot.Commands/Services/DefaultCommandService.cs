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
using Telegram.Bot;

namespace Masya.TelegramBot.Commands.Services
{
    public class DefaultCommandService<TCommandInfo, TAliasInfo> : ICommandService<TCommandInfo, TAliasInfo>
        where TAliasInfo : AliasInfo
        where TCommandInfo : CommandInfo<TAliasInfo>, new()
    {
        protected ContactHandlerInfo contactHandler;
        protected readonly List<TCommandInfo> commands;
        protected readonly List<CallbackInfo> callbacks;
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
            callbacks = new List<CallbackInfo>();
            this.services = services;
            this.logger = logger;
        }

        public virtual bool CheckCommandCondition(TCommandInfo commandInfo, Message message) =>
                commandInfo != null
                && commandInfo.MethodInfo != null
                && commandInfo.IsEnabled;

        protected virtual TCommandInfo GetCommand(string name, Message message) => commands.FirstOrDefault(cm => CommandFilter(cm, name));

        public virtual async Task HandleCallbackAsync(CallbackQuery callback)
        {
            var callbackSplittedData = callback.Data.Split(Options.CallbackDataSeparator);
            var callbackDataType = callbackSplittedData.First();
            var callbackDataParams = callbackSplittedData.Skip(1).ToArray();
            var callbackInfo = callbacks.FirstOrDefault(c => c.CallbackData.Equals(callbackDataType));
            var callbackParams = new CommandParts(callbackDataParams, Options);
            if (callbackInfo == null || callbackInfo.Handler == null)
            {
                return;
            }

            using var scope = services.CreateScope();
            var moduleInstance = ActivatorUtilities.CreateInstance(
                scope.ServiceProvider,
                callbackInfo.Handler.DeclaringType,
                Array.Empty<object>()
            );
            var propInfo = callbackInfo.Handler.DeclaringType.GetProperty("Context");
            var context = new DefaultCommandContext<TCommandInfo, TAliasInfo>(
                botService: BotService,
                commandService: this,
                chat: callback.Message.Chat,
                user: callback.From,
                message: callback.Message,
                callback: callback
            );
            propInfo.SetValue(moduleInstance, context);
            await (Task)callbackInfo.Handler.Invoke(moduleInstance, callbackParams.MatchParamTypes(callbackInfo.Handler));
            await BotService.Client.AnswerCallbackQueryAsync(callback.Id);
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
                var scope = services.CreateScope();
                var moduleInstance = ActivatorUtilities.CreateInstance(
                    scope.ServiceProvider,
                    commandInfo.MethodInfo.DeclaringType,
                    Array.Empty<object>()
                );
                var propInfo = commandInfo.MethodInfo.DeclaringType.GetProperty("Context");
                var context = new DefaultCommandContext<TCommandInfo, TAliasInfo>(
                    botService: BotService,
                    commandService: this,
                    chat: message.Chat,
                    user: message.From,
                    message: message
                );

                propInfo.SetValue(moduleInstance, context);
                await (Task)commandInfo.MethodInfo.Invoke(moduleInstance, parts.MatchParamTypes(commandInfo.MethodInfo));
            }
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
                            else if (IsValidCallbackHandler(method))
                            {
                                callbacks.Add(new CallbackInfo
                                {
                                    CallbackData = method.GetCustomAttribute<CallbackAttribute>().CallbackData,
                                    Handler = method
                                });
                            }
                        }
                    }
                }
            });
            logger.LogInformation($"Loaded {commands.Count} commands from assembly: " + assembly.GetName().Name);
        }

        public static bool IsValidCallbackHandler(MethodInfo method)
        {
            return method.GetCustomAttribute<CallbackAttribute>() != null
                && method.IsPublic
                && !method.IsAbstract
                && !method.IsGenericMethod
                && (method.ReturnType == typeof(Task) || method.ReturnType == typeof(Task<>));
        }

        public static TCommandInfo BuildCommandInfo(MethodInfo methodInfo) => new()
        {
            Name = methodInfo.GetCustomAttribute<CommandAttribute>()?.Name,
            Description = methodInfo.GetCustomAttribute<DescriptionAttribute>()?.Description,
            MethodInfo = methodInfo
        };

        public static bool IsValidCommand(MethodInfo method)
        {
            var commandAttr = method.GetCustomAttribute<CommandAttribute>();
            var regUserAttr = method.GetCustomAttribute<RegisterUserAttribute>();

            if (commandAttr != null && regUserAttr != null)
            {
                throw new FormatException("RegisterUserAttribute should not be used with CommandAttribute.");
            }

            return commandAttr != null
                && method.IsPublic
                && !method.IsAbstract
                && !method.IsGenericMethod
                && (method.ReturnType == typeof(Task) || method.ReturnType == typeof(Task<>));
        }

        public static bool IsContactHandler(MethodInfo method)
        {
            var methodParams = method.GetParameters();

            if (method.GetCustomAttribute<RegisterUserAttribute>() == null)
            {
                return false;
            }

            if (methodParams.Length != 1 || methodParams[0].ParameterType != typeof(Contact))
            {
                throw new FormatException(
                    string.Format(
                        "The contact handler should accept only one parameter of type {0}",
                        typeof(Contact).FullName
                    )
                );
            }

            return method.IsPublic
                && !method.IsAbstract
                && !method.IsGenericMethod
                && (method.ReturnType == typeof(Task) || method.ReturnType == typeof(Task<>));
        }

        public static bool IsModuleType(TypeInfo type) =>
                type.IsPublic
                && !type.IsAbstract
                && !type.IsGenericType
                && type.GetInterfaces().Any(
                    i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IModule<,>)
                );

        private static bool CommandFilter(TCommandInfo info, string commandName) =>
                info.Name.Equals(commandName)
                || info.Aliases.Any(a => a.Name.Equals(commandName) && a.IsEnabled);

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
            var context = new DefaultCommandContext<TCommandInfo, TAliasInfo>(
                botService: BotService,
                commandService: this,
                chat: message.Chat,
                user: message.From,
                message: message
            );

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
                    "Time is out. Please, repeat the command.",
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

            messageCollector.OnFinish += async (sender, args) =>
            {
                await (Task)method.Invoke(moduleInstance, result.ToArray());
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
                $"Please, specify {paramName}",
                cancellationToken: cancellationToken
                );
        }

        protected virtual async Task HandleContact(Message message)
        {
            logger.LogInformation(
                "Received a contact: {0} {1} {2}",
                message.Contact.FirstName,
                message.Contact.LastName,
                message.Contact.PhoneNumber
            );

            if (contactHandler == null) return;

            using var scope = services.CreateScope();
            var moduleInstance = ActivatorUtilities.CreateInstance(
                scope.ServiceProvider,
                contactHandler.MethodInfo.DeclaringType,
                Array.Empty<object>()
            );
            var propInfo = contactHandler.MethodInfo.DeclaringType.GetProperty("Context");
            var context = new DefaultCommandContext<TCommandInfo, TAliasInfo>(
                botService: BotService,
                commandService: this,
                chat: message.Chat,
                user: message.From,
                message: message
            );

            propInfo.SetValue(moduleInstance, context);
            await (Task)contactHandler.MethodInfo.Invoke(moduleInstance, new[] { message.Contact });
        }
    }
}