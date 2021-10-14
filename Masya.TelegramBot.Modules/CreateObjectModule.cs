using System;
using System.Linq;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Types;
using Masya.TelegramBot.DatabaseExtensions;
using Masya.TelegramBot.DatabaseExtensions.Abstractions;
using Masya.TelegramBot.DatabaseExtensions.Types;
using Masya.TelegramBot.DatabaseExtensions.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Masya.TelegramBot.Modules
{
    public sealed class CreateObjectModule : DatabaseModule
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IKeyboardGenerator _keyboards;
        private readonly IDistributedCache _cache;

        private const string CreateObjectProcessPrefix = "CreateObjectProcess_";

        public CreateObjectModule(
            ApplicationDbContext dbContext,
            IKeyboardGenerator keyboards,
            IDistributedCache cache
            )
        {
            _dbContext = dbContext;
            _keyboards = keyboards;
            _cache = cache;
        }

        [Command("/myobj")]
        public async Task MyObjectsCommandAsync()
        {
            var objects = await _dbContext.RealtyObjects
                .Where(ro => ro.Agent.TelegramAccountId == Context.User.Id)
                .ToListAsync();

            if (objects == null || objects.Count == 0)
            {
                await ReplyAsync(
                    "❌ You haven't created any realty objects yet.\n\nUse /create command to start *realty object creating procedure*.",
                    ParseMode.Markdown
                );
            }

            await SendObjectsAsync(objects);
        }

        [Command("/create")]
        public async Task CreateObjectCommandAsync()
        {
            var proc = await GetCurrentProcAsync();

            if (proc == null)
            {
                await ReplyAsync(
                    content: "Ok. First of all you should select the *category* of your object.",
                    replyMarkup: await _keyboards.SelectCategoriesAsync(),
                    parseMode: ParseMode.Markdown
                );
                return;
            }

            await ReplyAsync(
                content: MessageGenerator.GenerateCreateProcessMessage(proc),
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboards.ShowCreationMenu(proc)
            );
        }

        [Callback(CallbackDataTypes.SetObjectStreet)]
        public async Task HandleSetObjectStreetAsync(int streetId)
        {
            var street = await _dbContext.DirectoryItems.FirstOrDefaultAsync(di => di.Id == streetId);

            if (street == null)
            {
                return;
            }

            var proc = await GetCurrentProcAsync();

            if (proc == null)
            {
                return;
            }

            await Context.BotService.Client.EditMessageReplyMarkupAsync(
                chatId: Context.Chat.Id,
                messageId: Context.Message.MessageId,
                replyMarkup: null
            );
            proc.Street = street.Value;
            proc.StreetId = street.Id;
            await SaveCreationProcessAsync(proc);
            await SendCreationMenuMessageAsync(proc);
        }



        [Callback(CallbackDataTypes.SetObjectRegion)]
        public async Task HandleSetObjectRegion(int regionId = -1)
        {
            if (regionId == -1)
            {
                await EditMessageAsync(
                    replyMarkup: await _keyboards.ShowRegionsAsync()
                );
                return;
            }

            var region = await _dbContext.DirectoryItems.FirstOrDefaultAsync(di => di.Id == regionId);
            var proc = await GetCurrentProcAsync();

            if (region == null || proc == null)
            {
                return;
            }

            proc.District = region.Value;
            proc.DistrictId = region.Id;
            await SaveCreationProcessAsync(proc);
            await EditMessageAsync(
                text: MessageGenerator.GenerateCreateProcessMessage(proc),
                replyMarkup: _keyboards.ShowCreationMenu(proc),
                parseMode: ParseMode.Markdown
            );
        }

        private static string Capitalize(string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentNullException(nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };

        private async Task HandleDirectoryItems(DirectoryType itemType, string fieldName, int itemId = -1)
        {
            if (itemId == -1)
            {
                await EditMessageAsync(
                    replyMarkup: await _keyboards.SelectDirectoryItems(itemType)
                );
                return;
            }

            var item = await _dbContext.DirectoryItems.FirstOrDefaultAsync(
                di => di.Id == itemId && di.DirectoryId == (int)itemType
            );

            var proc = await GetCurrentProcAsync();

            if (item == null || proc == null)
            {
                return;
            }

            var prop = proc.GetType().GetProperties().First(p => p.Name == fieldName);
            prop.SetValue(proc, item.Value);
            prop = proc.GetType().GetProperties().First(p => p.Name == fieldName + "Id");
            prop.SetValue(proc, item.Id);

            await SaveCreationProcessAsync(proc);
            await EditMessageAsync(
                text: MessageGenerator.GenerateCreateProcessMessage(proc),
                replyMarkup: _keyboards.ShowCreationMenu(proc),
                parseMode: ParseMode.Markdown
            );
        }

        private async Task HandleNumericValueInput(
            CreateProcess proc,
            string propName,
            string displayName,
            Func<int, bool> validate
        )
        {
            Context.BotService.TryRemoveCollector(Context.Chat);
            await EditMessageAsync();
            var collector = Context.BotService.CreateMessageCollector(Context.Chat, TimeSpan.FromMinutes(2));
            collector.Collect(m => m.Text);

            collector.OnStart += async (sender, e) =>
            {
                await ReplyAsync($"Please, send a valid {displayName} for your object.");
            };

            collector.OnMessageReceived += async (sender, e) =>
            {
                var parsed = int.TryParse(e.Message.Text, out int value);
                if (!parsed || !validate(value))
                {
                    await ReplyAsync($"❌ You have provided an invalid {displayName}.");
                    collector.Finish();
                    return;
                }

                int? nullableValue = value;
                var prop = proc.GetType().GetProperties().FirstOrDefault(p => p.Name == propName);

                if (prop == null || !nullableValue.HasValue)
                {
                    throw new InvalidOperationException("Invalid property name.");
                }

                prop.SetValue(proc, nullableValue.Value);
                await SaveCreationProcessAsync(proc);
                await ReplyAsync($"✅ {Capitalize(displayName)} has been set successfully!");
                await SendCreationMenuMessageAsync(proc);

                collector.Finish();
                return;
            };

            collector.OnMessageTimeout += async (sender, e) =>
            {
                var proc = await GetCurrentProcAsync();
                await ReplyAsync("⌛ The time is out. Please, try again.");
                await SendCreationMenuMessageAsync(proc);
            };

            collector.Start();
        }

        [Callback(CallbackDataTypes.SetObjectPrice)]
        public async Task HandleSetObjectPriceAsync()
        {
            await HandleNumericValueInput(
                await GetCurrentProcAsync(),
                "Price",
                "price",
                (price) => price > 0
            );
        }

        [Callback(CallbackDataTypes.SetObjectLotArea)]
        public async Task HandleSetObjectLotAreaAsync()
        {
            await HandleNumericValueInput(
                await GetCurrentProcAsync(),
                "LotArea",
                "lot area",
                (area) => area > 0
            );
        }

        [Callback(CallbackDataTypes.SetObjectTotalArea)]
        public async Task HandleSetObjectTotalAreaAsync()
        {
            await HandleNumericValueInput(
                await GetCurrentProcAsync(),
                "TotalArea",
                "total area",
                (area) => area > 0
            );
        }

        [Callback(CallbackDataTypes.SetObjectKitchenArea)]
        public async Task HandleSetObjectKitchenAreaAsync()
        {
            await HandleNumericValueInput(
                await GetCurrentProcAsync(),
                "KitchenSpace",
                "kitchen area",
                (area) => area > 0
            );
        }

        [Callback(CallbackDataTypes.SetObjectLivingArea)]
        public async Task HandleSetObjectLivingAreaAsync()
        {
            await HandleNumericValueInput(
                await GetCurrentProcAsync(),
                "LivingSpace",
                "living area",
                (area) => area > 0
            );
        }

        [Callback(CallbackDataTypes.CancelSetObjectStreet)]
        public async Task HandleCancelSetStreetAsync()
        {
            Context.BotService.TryRemoveCollector(Context.Chat);
            var proc = await GetCurrentProcAsync();
            await SendCreationMenuMessageAsync(proc);
        }

        [Callback(CallbackDataTypes.SetObjectDescription)]
        public async Task HandleSetObjectDescrAsync()
        {
            Context.BotService.TryRemoveCollector(Context.Chat);
            await EditMessageAsync();
            var collector = Context.BotService.CreateMessageCollector(Context.Chat, TimeSpan.FromMinutes(2));
            collector.Collect(m => m.Text);

            collector.OnStart += async (sender, e) =>
            {
                await ReplyAsync("Please, send a valid description for your object.");
            };

            collector.OnMessageReceived += async (sender, e) =>
            {
                var proc = await GetCurrentProcAsync();

                if (proc == null)
                {
                    collector.Finish();
                    return;
                }

                proc.Description = e.Message.Text;
                await SaveCreationProcessAsync(proc);
                await ReplyAsync("✅ Description has been set successfully!");
                await SendCreationMenuMessageAsync(proc);

                collector.Finish();
                return;
            };

            collector.OnMessageTimeout += async (sender, e) =>
            {
                var proc = await GetCurrentProcAsync();
                await ReplyAsync("⌛ The time is out. Please, try again.");
                await SendCreationMenuMessageAsync(proc);
            };

            collector.Start();
        }

        private async Task<CreateProcess> GetCurrentProcAsync()
            => await _cache.GetRecordAsync<CreateProcess>(CreateObjectProcessPrefix + Context.User.Id);

        private async Task SaveCreationProcessAsync(CreateProcess proc)
        {
            await _cache.SetRecordAsync(
                CreateObjectProcessPrefix + Context.User.Id,
                proc,
                TimeSpan.FromDays(1),
                TimeSpan.FromHours(1)
            );
        }

        private async Task SendCreationMenuMessageAsync(CreateProcess proc)
        {
            await ReplyAsync(
                content: MessageGenerator.GenerateCreateProcessMessage(proc),
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboards.ShowCreationMenu(proc)
            );
        }

        [Callback(CallbackDataTypes.CancelObjectCreation)]
        public async Task HandleCancelObjectCreation()
        {
            await _cache.RemoveAsync(CreateObjectProcessPrefix + Context.User.Id);
            await ReplyAsync(
                "✅ Removed object from creation process.\n\n*Use /create command to start creating again.*",
                ParseMode.Markdown
            );
        }

        [Callback(CallbackDataTypes.SetObjectType)]
        public async Task HandleSetObjectTypeAsync(int categoryId = -1)
        {
            if (categoryId == -1)
            {
                await EditMessageAsync(
                    replyMarkup: await _keyboards.SelectCategoriesAsync()
                );
                return;
            }

            var proc = await GetCurrentProcAsync();
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
            {
                return;
            }

            if (proc == null)
            {
                proc = new CreateProcess();
            }

            proc.CategoryId = categoryId;
            proc.Category = category.Name;

            await SaveCreationProcessAsync(proc);
            await EditMessageAsync(
                text: MessageGenerator.GenerateCreateProcessMessage(proc),
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboards.ShowCreationMenu(proc)
            );
        }
    }
}