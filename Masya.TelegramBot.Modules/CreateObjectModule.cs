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
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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
            var proc = await _cache.GetRecordAsync<CreateProcess>(CreateObjectProcessPrefix + Context.User.Id);

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
                if (string.IsNullOrEmpty(e.Message.Text))
                {
                    await ReplyAsync("❌ You have provided an invalid description.");
                    collector.Finish();
                    return;
                }

                var proc = await _cache.GetRecordAsync<CreateProcess>(CreateObjectProcessPrefix + Context.User.Id);

                if (await TrySetObjectDescription(proc, e.Message.Text))
                {
                    await ReplyAsync("✅ Description has been set successfully!");
                    await SendCreationMenuMessageAsync(proc);
                }

                collector.Finish();
                return;
            };

            collector.OnMessageTimeout += async (sender, e) =>
            {
                var proc = await _cache.GetRecordAsync<CreateProcess>(CreateObjectProcessPrefix + Context.User.Id);
                await ReplyAsync("⌛ The time is out. Please, try again.");
                await SendCreationMenuMessageAsync(proc);
            };

            collector.Start();
        }

        private async Task SaveCreationProcessAsync(CreateProcess proc)
        {
            await _cache.SetRecordAsync(
                CreateObjectProcessPrefix + Context.User.Id,
                proc,
                TimeSpan.FromDays(1),
                TimeSpan.FromHours(1)
            );
        }

        private async Task<bool> TrySetObjectDescription(CreateProcess proc, string description)
        {
            if (proc == null)
            {
                return false;
            }

            proc.Description = description;
            await SaveCreationProcessAsync(proc);
            return true;
        }

        private async Task SendCreationMenuMessageAsync(CreateProcess proc)
        {
            await EditMessageAsync(
                text: MessageGenerator.GenerateCreateProcessMessage(proc),
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboards.ShowCreationMenu(proc)
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

            var proc = await _cache.GetRecordAsync<CreateProcess>(CreateObjectProcessPrefix + Context.User.Id);
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
            await SendCreationMenuMessageAsync(proc);
        }
    }
}