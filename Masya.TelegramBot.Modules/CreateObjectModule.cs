using System;
using System.Linq;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Models;
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

        private const int MaxFloors = 24;
        private const int MaxRooms = 8;

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

            await SendObjectsAsync(
                results: objects,
                isEditMode: true
            );
        }

        [Command("/create")]
        public async Task CreateObjectCommandAsync()
        {
            var proc = await GetCurrentProcAsync(CreateProcess.CreateObjectProcessPrefix);

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

        private static void MapCreateProcToObj(CreateProcess proc, RealtyObject obj, bool isCreating)
        {
            obj.AgentId = proc.AgentId.Value;
            obj.CategoryId = proc.CategoryId.Value;
            obj.StreetId = proc.StreetId.Value;
            if (proc.WallMaterialId.HasValue) obj.WallMaterialId = proc.WallMaterialId.Value;
            if (proc.StateId.HasValue) obj.StateId = proc.StateId.Value;
            if (proc.DistrictId.HasValue) obj.DistrictId = proc.DistrictId.Value;
            obj.Price = proc.Price.Value;
            if (proc.TotalArea.HasValue) obj.TotalArea = proc.TotalArea.Value;
            if (proc.LotArea.HasValue) obj.LotArea = proc.LotArea.Value;
            if (proc.LivingSpace.HasValue) obj.LivingSpace = proc.LivingSpace.Value;
            if (proc.KitchenSpace.HasValue) obj.KitchenSpace = proc.KitchenSpace.Value;
            obj.Description = proc.Description;
            if (proc.Rooms.HasValue) obj.Rooms = proc.Rooms.Value;
            obj.Phone = proc.Phone;
            obj.EditedAt = DateTime.Now;

            if (isCreating)
            {
                obj.CreatedAt = DateTime.Now;
            }
        }

        private static void MapObjToCreateProc(RealtyObject obj, CreateProcess proc)
        {
            proc.Id = obj.Id;
            proc.AgentId = obj.AgentId.Value;
            proc.CategoryId = obj.CategoryId;
            proc.Category = obj.Category.Name;
            proc.StreetId = obj.StreetId.Value;
            proc.Street = obj.Street.Value;
            proc.DistrictId = obj.DistrictId.Value;
            proc.District = obj.District.Value;
            proc.Price = obj.Price.Value;
            if (obj.WallMaterialId.HasValue)
            {
                proc.WallMaterialId = obj.WallMaterialId.Value;
                proc.WallMaterial = obj.WallMaterial.Value;
            }
            if (obj.StateId.HasValue)
            {
                proc.StateId = obj.StateId.Value;
                proc.State = obj.State.Value;
            }
            if (obj.TotalArea.HasValue) proc.TotalArea = (int)obj.TotalArea.Value;
            if (obj.LotArea.HasValue) proc.LotArea = (int)obj.LotArea.Value;
            if (obj.LivingSpace.HasValue) proc.LivingSpace = (int)obj.LivingSpace.Value;
            if (obj.KitchenSpace.HasValue) proc.KitchenSpace = (int)obj.KitchenSpace.Value;
            if (obj.Rooms.HasValue) proc.Rooms = obj.Rooms.Value;
            proc.Description = obj.Description;
            proc.Phone = obj.Phone;
        }

        [Callback(CallbackDataTypes.EditObject)]
        public async Task HandleEditObjectAsync(int objId)
        {
            var target = await _dbContext.RealtyObjects.FirstOrDefaultAsync(ro => ro.Id == objId);
            if (target == null)
            {
                return;
            }

            var proc = new CreateProcess();
            MapObjToCreateProc(target, proc);
            await SaveCreationProcessAsync(proc, CreateProcess.EditObjectProcessPrefix);
            await SendCreationMenuMessageAsync(proc, true);
        }

        [Callback(CallbackDataTypes.RemoveObject)]
        public async Task HandleRemoveObjectAsync(int objId)
        {
            var target = await _dbContext.RealtyObjects.FirstOrDefaultAsync(ro => ro.Id == objId);

            if (target != null)
            {
                _dbContext.RealtyObjects.Remove(target);
                await _dbContext.SaveChangesAsync();
                await EditMessageAsync("✅ Object has been successfully removed.");
            }
        }

        [Callback(CallbackDataTypes.SaveObject)]
        public async Task HandleSaveObjectAsync(string prefix = CreateProcess.CreateObjectProcessPrefix)
        {
            var proc = await GetCurrentProcAsync(prefix);
            if (proc == null)
            {
                return;
            }

            await EditMessageAsync("⏳ Saving object...");

            await _cache.RemoveAsync(CreateProcess.CreateObjectProcessPrefix + Context.User.Id);

            if (proc.Id.HasValue)
            {
                var target = await _dbContext.RealtyObjects.FirstOrDefaultAsync(ro => ro.Id == proc.Id.Value);
                if (target == null)
                {
                    return;
                }

                MapCreateProcToObj(proc, target, false);
            }
            else
            {
                var target = new RealtyObject();
                MapCreateProcToObj(proc, target, true);
                _dbContext.RealtyObjects.Add(target);
            }

            await _dbContext.SaveChangesAsync();

            await EditMessageAsync("✅ Object has been successfully saved!");
        }

        public static string GetCachePrefix(CreateProcess proc) =>
            proc.Id.HasValue
            ? CreateProcess.EditObjectProcessPrefix
            : CreateProcess.CreateObjectProcessPrefix;

        [Callback(CallbackDataTypes.SetObjectStreet)]
        public async Task HandleSetObjectStreetAsync(string prefix = CreateProcess.CreateObjectProcessPrefix, int streetId = -1)
        {
            var street = await _dbContext.DirectoryItems.FirstOrDefaultAsync(di => di.Id == streetId);

            if (street == null)
            {
                return;
            }

            var proc = await GetCurrentProcAsync(prefix);

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
            await SaveCreationProcessAsync(proc, prefix);
            await SendCreationMenuMessageAsync(proc, proc.Id.HasValue);
        }

        [Callback(CallbackDataTypes.SetObjectState)]
        public async Task HandleSetObjectStateAsync(string prefix = CreateProcess.CreateObjectProcessPrefix, int stateId = -1)
        {
            await HandleDirectoryItemsAsync(DirectoryType.State, "State", stateId, prefix);
        }

        [Callback(CallbackDataTypes.SetObjectWallsMaterial)]
        public async Task HandleSetObjectWallMaterialAsync(string prefix = CreateProcess.CreateObjectProcessPrefix, int materialId = -1)
        {
            await HandleDirectoryItemsAsync(DirectoryType.Material, "WallMaterial", materialId, prefix);
        }

        [Callback(CallbackDataTypes.SetObjectRegion)]
        public async Task HandleSetObjectRegionAsync(string prefix = CreateProcess.CreateObjectProcessPrefix, int regionId = -1)
        {
            if (regionId == -1)
            {
                await EditMessageAsync(
                    replyMarkup: await _keyboards.ShowRegionsAsync(prefix)
                );
                return;
            }

            var region = await _dbContext.DirectoryItems.FirstOrDefaultAsync(di => di.Id == regionId);
            var proc = await GetCurrentProcAsync(prefix);

            if (region == null || proc == null)
            {
                return;
            }

            proc.District = region.Value;
            proc.DistrictId = region.Id;
            await SaveCreationProcessAsync(proc, GetCachePrefix(proc));
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

        private async Task HandleDirectoryItemsAsync(DirectoryType itemType, string fieldName, int itemId = -1, string prefix = CreateProcess.CreateObjectProcessPrefix)
        {
            if (itemId == -1)
            {
                await EditMessageAsync(
                    replyMarkup: await _keyboards.SelectDirectoryItems(itemType, prefix)
                );
                return;
            }

            var item = await _dbContext.DirectoryItems.FirstOrDefaultAsync(
                di => di.Id == itemId && di.DirectoryId == (int)itemType
            );

            var proc = await GetCurrentProcAsync(prefix);

            if (item == null || proc == null)
            {
                return;
            }

            var prop = proc.GetType().GetProperties().First(p => p.Name == fieldName);
            prop.SetValue(proc, item.Value);
            prop = proc.GetType().GetProperties().First(p => p.Name == fieldName + "Id");
            prop.SetValue(proc, item.Id);

            await SaveCreationProcessAsync(proc, GetCachePrefix(proc));
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
            Func<int, bool> validate,
            string prefix
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
                await SaveCreationProcessAsync(proc, GetCachePrefix(proc));
                await ReplyAsync($"✅ {Capitalize(displayName)} has been set successfully!");
                await SendCreationMenuMessageAsync(proc, proc.Id.HasValue);

                collector.Finish();
                return;
            };

            collector.OnMessageTimeout += async (sender, e) =>
            {
                var proc = await GetCurrentProcAsync(prefix);
                await ReplyAsync("⌛ The time is out. Please, try again.");
                await SendCreationMenuMessageAsync(proc, proc.Id.HasValue);
            };

            collector.Start();
        }

        [Callback(CallbackDataTypes.SetObjectPrice)]
        public async Task HandleSetObjectPriceAsync(string prefix = CreateProcess.CreateObjectProcessPrefix)
        {
            await HandleNumericValueInput(
                await GetCurrentProcAsync(prefix),
                "Price",
                "price",
                (price) => price > 0,
                prefix
            );
        }

        [Callback(CallbackDataTypes.SetObjectFloor)]
        public async Task HandleSetObjectFloorAsync(string prefix = CreateProcess.CreateObjectProcessPrefix, int floor = -1)
        {
            if (floor < 1 || floor > 24)
            {
                await EditMessageAsync(
                    replyMarkup: _keyboards.SelectNumericValues(CallbackDataTypes.SetObjectFloor, MaxFloors, prefix)
                );
                return;
            }

            var proc = await GetCurrentProcAsync(prefix);
            proc.Floor = floor;
            await SaveCreationProcessAsync(proc, GetCachePrefix(proc));
            await EditMessageAsync(
                text: MessageGenerator.GenerateCreateProcessMessage(proc),
                replyMarkup: _keyboards.ShowCreationMenu(proc),
                parseMode: ParseMode.Markdown
            );
        }

        [Callback(CallbackDataTypes.SetObjectTotalFloors)]
        public async Task HandleSetObjectTotalFloorsAsync(string prefix = CreateProcess.CreateObjectProcessPrefix, int totalFloors = -1)
        {
            if (totalFloors < 1 || totalFloors > MaxFloors)
            {
                await EditMessageAsync(
                    replyMarkup: _keyboards.SelectNumericValues(CallbackDataTypes.SetObjectTotalFloors, MaxFloors, prefix)
                );
                return;
            }

            var proc = await GetCurrentProcAsync(prefix);
            proc.TotalFloors = totalFloors;
            await SaveCreationProcessAsync(proc, GetCachePrefix(proc));
            await EditMessageAsync(
                text: MessageGenerator.GenerateCreateProcessMessage(proc),
                replyMarkup: _keyboards.ShowCreationMenu(proc),
                parseMode: ParseMode.Markdown
            );
        }

        [Callback(CallbackDataTypes.SetObjectRoomsCount)]
        public async Task HandleSetObjectRoomsCountAsync(string prefix = CreateProcess.CreateObjectProcessPrefix, int roomsCount = -1)
        {
            if (roomsCount < 1 || roomsCount > MaxRooms)
            {
                await EditMessageAsync(
                    replyMarkup: _keyboards.SelectNumericValues(CallbackDataTypes.SetObjectRoomsCount, MaxRooms, prefix)
                );
                return;
            }

            var proc = await GetCurrentProcAsync(prefix);
            proc.Rooms = roomsCount;
            await SaveCreationProcessAsync(proc, GetCachePrefix(proc));
            await EditMessageAsync(
                text: MessageGenerator.GenerateCreateProcessMessage(proc),
                replyMarkup: _keyboards.ShowCreationMenu(proc),
                parseMode: ParseMode.Markdown
            );
        }

        [Callback(CallbackDataTypes.SetObjectLotArea)]
        public async Task HandleSetObjectLotAreaAsync(string prefix = CreateProcess.CreateObjectProcessPrefix)
        {
            await HandleNumericValueInput(
                await GetCurrentProcAsync(prefix),
                "LotArea",
                "lot area",
                (area) => area > 0,
                prefix
            );
        }

        [Callback(CallbackDataTypes.SetObjectTotalArea)]
        public async Task HandleSetObjectTotalAreaAsync(string prefix = CreateProcess.CreateObjectProcessPrefix)
        {
            await HandleNumericValueInput(
                await GetCurrentProcAsync(prefix),
                "TotalArea",
                "total area",
                (area) => area > 0,
                prefix
            );
        }

        [Callback(CallbackDataTypes.SetObjectKitchenArea)]
        public async Task HandleSetObjectKitchenAreaAsync(string prefix = CreateProcess.CreateObjectProcessPrefix)
        {
            await HandleNumericValueInput(
                await GetCurrentProcAsync(prefix),
                "KitchenSpace",
                "kitchen area",
                (area) => area > 0,
                prefix
            );
        }

        [Callback(CallbackDataTypes.SetObjectLivingArea)]
        public async Task HandleSetObjectLivingAreaAsync(string prefix = CreateProcess.CreateObjectProcessPrefix)
        {
            await HandleNumericValueInput(
                await GetCurrentProcAsync(prefix),
                "LivingSpace",
                "living area",
                (area) => area > 0,
                prefix
            );
        }

        [Callback(CallbackDataTypes.CancelSetObjectStreet)]
        public async Task HandleCancelSetStreetAsync(string prefix = CreateProcess.CreateObjectProcessPrefix)
        {
            Context.BotService.TryRemoveCollector(Context.Chat);
            var proc = await GetCurrentProcAsync(prefix);
            await SendCreationMenuMessageAsync(proc, proc.Id.HasValue);
        }

        [Callback(CallbackDataTypes.SetObjectDescription)]
        public async Task HandleSetObjectDescrAsync(string prefix = CreateProcess.CreateObjectProcessPrefix)
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
                var proc = await GetCurrentProcAsync(prefix);

                if (proc == null)
                {
                    collector.Finish();
                    return;
                }

                proc.Description = e.Message.Text;
                await SaveCreationProcessAsync(proc, GetCachePrefix(proc));
                await ReplyAsync("✅ Description has been set successfully!");
                await SendCreationMenuMessageAsync(proc, proc.Id.HasValue);

                collector.Finish();
                return;
            };

            collector.OnMessageTimeout += async (sender, e) =>
            {
                var proc = await GetCurrentProcAsync(prefix);
                await ReplyAsync("⌛ The time is out. Please, try again.");
                await SendCreationMenuMessageAsync(proc, proc.Id.HasValue);
            };

            collector.Start();
        }

        private async Task<CreateProcess> GetCurrentProcAsync(string prefix)
            => await _cache.GetRecordAsync<CreateProcess>(prefix + Context.User.Id);

        private async Task SaveCreationProcessAsync(CreateProcess proc, string prefix = CreateProcess.CreateObjectProcessPrefix)
        {
            await _cache.SetRecordAsync(
                prefix + Context.User.Id,
                proc,
                TimeSpan.FromDays(1),
                TimeSpan.FromHours(1)
            );
        }

        private async Task SendCreationMenuMessageAsync(CreateProcess proc, bool isEditMode = false)
        {
            await ReplyAsync(
                content: MessageGenerator.GenerateCreateProcessMessage(proc),
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboards.ShowCreationMenu(proc, isEditMode)
            );
        }

        [Callback(CallbackDataTypes.CancelObjectEditing)]
        public async Task HandleCancelObjectEditingAsync()
        {
            await _cache.RemoveAsync(CreateProcess.EditObjectProcessPrefix + Context.User.Id);
            await ReplyAsync("✅ Object editing process has been canceled.");
        }

        [Callback(CallbackDataTypes.CancelObjectCreation)]
        public async Task HandleCancelObjectCreationAsync()
        {
            await _cache.RemoveAsync(CreateProcess.CreateObjectProcessPrefix + Context.User.Id);
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

            var proc = await GetCurrentProcAsync(CreateProcess.CreateObjectProcessPrefix);
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            var owner = await _dbContext.Users
                .Select(u => new { u.Id, u.TelegramAccountId, u.TelegramPhoneNumber })
                .FirstOrDefaultAsync(u => u.TelegramAccountId == Context.User.Id);

            if (category == null || owner == null)
            {
                return;
            }

            if (proc == null)
            {
                proc = new CreateProcess();
            }

            proc.CategoryId = categoryId;
            proc.Category = category.Name;
            proc.AgentId = owner.Id;
            proc.Phone = owner.TelegramPhoneNumber;

            await SaveCreationProcessAsync(proc, GetCachePrefix(proc));
            await EditMessageAsync(
                text: MessageGenerator.GenerateCreateProcessMessage(proc),
                parseMode: ParseMode.Markdown,
                replyMarkup: _keyboards.ShowCreationMenu(proc)
            );
        }
    }
}
