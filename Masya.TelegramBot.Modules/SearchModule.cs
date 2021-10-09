using Masya.TelegramBot.Commands.Attributes;
using Masya.TelegramBot.DatabaseExtensions;
using Masya.TelegramBot.DatabaseExtensions.Abstractions;
using Telegram.Bot;
using System.Linq;
using System.Threading.Tasks;
using Masya.TelegramBot.DataAccess;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Masya.TelegramBot.DataAccess.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO;

namespace Masya.TelegramBot.Modules
{
    public sealed class SearchModule : DatabaseModule
    {
        private readonly IKeyboardGenerator _keyboards;
        private readonly ApplicationDbContext _dbContext;
        private readonly IDistributedCache _cache;
        private readonly ILogger<SearchModule> _logger;

        private const string SearchProcessPrefix = "SearchProcess_";

        public SearchModule(
            ApplicationDbContext dbContext,
            IKeyboardGenerator keyboards,
            ILogger<SearchModule> logger,
            IDistributedCache cache
        )
        {
            _dbContext = dbContext;
            _keyboards = keyboards;
            _cache = cache;
            _logger = logger;
        }

        [Command("/search")]
        public async Task SearchAsync()
        {
            var userSettings = await _dbContext.UserSettings
                .AsNoTracking()
                .AsSplitQuery()
                .Include(us => us.SelectedCategories)
                .Include(us => us.SelectedRegions)
                .Include(us => us.Rooms)
                .Select(us => us)
                .FirstOrDefaultAsync(us => us.User.TelegramAccountId == Context.User.Id);

            if (userSettings == null)
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(us => us.TelegramAccountId == Context.User.Id);
                if (user == null)
                {
                    return;
                }

                userSettings = new UserSettings();
                user.UserSettings = userSettings;
                await _dbContext.SaveChangesAsync();
            }

            await ReplyAsync(
                content: MessageGenerators.GenerateSearchSettingsMessage(userSettings),
                parseMode: ParseMode.Markdown,
                replyMarkup: await _keyboards.InlineSearchAsync(CallbackDataTypes.MainMenu)
            );
        }

        [Callback(CallbackDataTypes.ExecuteSearch)]
        public async Task HandleExecuteSearchAsync()
        {
            try
            {
                string cacheKey = SearchProcessPrefix + Context.User.Id;
                int objLimit = Context.CommandService.Options.ObjectsSentLimit;
                var searchProcess = await _cache.GetRecordAsync<SearchProcess>(cacheKey);

                if (searchProcess == null)
                {
                    var message = await ReplyAsync("🔍 Searching for realty objects...");
                    var userSettings = await _dbContext.UserSettings
                        .AsNoTracking()
                        .AsSplitQuery()
                        .Include(us => us.SelectedCategories)
                        .Include(us => us.SelectedRegions)
                        .Include(us => us.Rooms)
                        .Select(us => us)
                        .FirstOrDefaultAsync(us => us.User.TelegramAccountId == Context.User.Id);

                    var allObjects = await _dbContext.RealtyObjects
                        .AsNoTracking()
                        .AsSplitQuery()
                        .Include(ro => ro.Images)
                        .Include(ro => ro.Category)
                        .Include(ro => ro.District)
                        .Include(ro => ro.WallMaterial)
                        .Include(ro => ro.State)
                        .Include(ro => ro.Street)
                        .OrderBy(ro => ro.Id)
                        .ToListAsync();

                    var results = allObjects
                        .Where(ro => userSettings.SelectedCategories.Any(sc => sc.Id == ro.CategoryId))
                        .Where(ro => userSettings.SelectedRegions.Any(sr => sr.Id == ro.DistrictId))
                        .Where(ro => !userSettings.MinPrice.HasValue || ro.Price >= userSettings.MinPrice.Value)
                        .Where(ro => !userSettings.MaxPrice.HasValue || ro.Price <= userSettings.MaxPrice.Value)
                        .Where(ro => !userSettings.MinFloor.HasValue || ro.Floor >= userSettings.MinFloor.Value)
                        .Where(ro => !userSettings.MaxFloor.HasValue || ro.Floor <= userSettings.MaxFloor.Value)
                        .Where(ro => userSettings.Rooms.Any(r => r.RoomsCount == ro.Floor))
                        .ToList();

                    if (results.Count == 0)
                    {
                        await Context.BotService.Client.EditMessageTextAsync(
                            chatId: Context.Chat.Id,
                            messageId: message.MessageId,
                            text: "❌ No result were found for your search settings.\n*Configure search settings:* /search",
                            parseMode: ParseMode.Markdown
                        );
                        return;
                    }

                    await Context.BotService.Client.EditMessageTextAsync(
                            chatId: Context.Chat.Id,
                            messageId: message.MessageId,
                            text: string.Format("✅ Found *{0}* objects.", results.Count),
                            parseMode: ParseMode.Markdown
                        );

                    if (results.Count > objLimit)
                    {
                        searchProcess = new SearchProcess
                        {
                            TelegramId = Context.User.Id,
                            RealtyObjects = results.Skip(objLimit),
                            ItemsSentCount = objLimit,
                            TotalItemsFound = results.Count,
                        };

                        await _cache.SetRecordAsync(
                            cacheKey,
                            searchProcess,
                            TimeSpan.FromMinutes(10)
                        );

                        await SendResultsAsync(results.Take(searchProcess.ItemsSentCount).ToList());
                        await ReplyAsync(
                            content: string.Format(
                                "Sent *{0} of {1}* results.\nThe button below will be unavailable in 10 minutes.",
                                searchProcess.ItemsSentCount,
                                searchProcess.TotalItemsFound
                            ),
                            replyMarkup: new InlineKeyboardMarkup(
                                InlineKeyboardButton.WithCallbackData("🔍See more", CallbackDataTypes.ExecuteSearch)
                            ),
                            parseMode: ParseMode.Markdown
                        );
                        return;
                    }

                    await SendResultsAsync(results.Take(objLimit).ToList());
                    await ReplyAsync(
                        "There are no more results with such search settings.\n*Configure search settings:* /search",
                        ParseMode.Markdown
                    );
                    return;
                }

                if (searchProcess.RealtyObjects.Count() > objLimit)
                {
                    await _cache.RemoveAsync(cacheKey);

                    searchProcess.ItemsSentCount += objLimit;
                    searchProcess.RealtyObjects = searchProcess.RealtyObjects.Skip(objLimit);
                    await _cache.SetRecordAsync(
                        cacheKey,
                        searchProcess,
                        TimeSpan.FromMinutes(10)
                    );

                    await SendResultsAsync(
                        searchProcess.RealtyObjects
                            .Take(objLimit)
                            .ToList()
                    );

                    await ReplyAsync(
                        content: string.Format(
                            "Sent *{0} of {1}* results.",
                            searchProcess.ItemsSentCount,
                            searchProcess.TotalItemsFound
                        ),
                        replyMarkup: new InlineKeyboardMarkup(
                            InlineKeyboardButton.WithCallbackData("🔍See more", CallbackDataTypes.ExecuteSearch)
                        ),
                        parseMode: ParseMode.Markdown
                    );
                    return;
                }

                if (searchProcess.RealtyObjects.Count() <= objLimit)
                {
                    await _cache.RemoveAsync(cacheKey);
                    await SendResultsAsync(
                        searchProcess.RealtyObjects
                            .Skip(searchProcess.ItemsSentCount)
                            .Take(objLimit)
                            .ToList()
                    );
                    await ReplyAsync(
                        "There are no more results with such search settings.\n*Configure search settings:* /search",
                        ParseMode.Markdown
                    );
                    return;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }

        [Callback(CallbackDataTypes.ChangeSettings)]
        public async Task HandleChangeSettingsAsync()
        {
            await EditMessageAsync(
                replyMarkup: await _keyboards.InlineSearchAsync(CallbackDataTypes.ChangeSettings)
            );
        }

        [Callback(CallbackDataTypes.MainMenu)]
        public async Task HandleMenuAsync()
        {
            await EditMessageAsync(
                replyMarkup: await _keyboards.InlineSearchAsync(CallbackDataTypes.MainMenu)
            );
        }

        [Callback(CallbackDataTypes.UpdateRooms)]
        public async Task HandleChangeRoomsAsync(int selectedRoomsId = -1)
        {
            var userSettings = await _dbContext.UserSettings
                .AsSplitQuery()
                .Include(us => us.SelectedCategories)
                .Include(us => us.SelectedRegions)
                .Include(us => us.Rooms)
                .Select(us => us)
                .FirstOrDefaultAsync(us => us.User.TelegramAccountId == Context.User.Id);

            if (userSettings == null)
            {
                return;
            }

            if (selectedRoomsId != -1)
            {
                var selectedRooms = userSettings.Rooms.FirstOrDefault(c => c.Id == selectedRoomsId);

                if (selectedRooms == null)
                {
                    userSettings.Rooms.Add(
                        _dbContext.Rooms.First(c => c.Id == selectedRoomsId)
                    );
                }
                else
                {
                    userSettings.Rooms.Remove(selectedRooms);
                }
                await _dbContext.SaveChangesAsync();
            }

            var rooms = await _keyboards.InlineSearchAsync(
                CallbackDataTypes.UpdateRooms, userSettings
            );

            if (!rooms.InlineKeyboard.Any())
            {
                await Context.BotService.Client.AnswerCallbackQueryAsync(
                    callbackQueryId: Context.Callback.Id,
                    text: "There are no rooms yet."
                );
                return;
            }

            await EditMessageAsync(
                text: selectedRoomsId != -1 ? MessageGenerators.GenerateSearchSettingsMessage(userSettings) : null,
                replyMarkup: rooms,
                parseMode: ParseMode.Markdown
            );
        }

        [Callback(CallbackDataTypes.UpdatePrice)]
        public async Task HandleUpdatePriceAsync(string type = null, int selectedValue = -1)
        {
            var userSettings = await _dbContext.UserSettings
                .AsSplitQuery()
                .Include(us => us.SelectedCategories)
                .Include(us => us.SelectedRegions)
                .Include(us => us.Rooms)
                .Select(us => us)
                .FirstOrDefaultAsync(us => us.User.TelegramAccountId == Context.User.Id);

            if (userSettings == null)
            {
                return;
            }

            if (selectedValue != -1 && !string.IsNullOrEmpty(type))
            {
                switch (type)
                {
                    case KeyboardGenerator.MaxOperation:
                        userSettings.MaxPrice = userSettings.MaxPrice == selectedValue ? null : selectedValue;
                        break;

                    case KeyboardGenerator.MinOperation:
                        userSettings.MinPrice = userSettings.MinPrice == selectedValue ? null : selectedValue;
                        break;

                    default:
                        break;
                }
                await _dbContext.SaveChangesAsync();
            }

            var prices = await _keyboards.InlineSearchAsync(CallbackDataTypes.UpdatePrice, userSettings);

            if (!prices.InlineKeyboard.Any())
            {
                await Context.BotService.Client.AnswerCallbackQueryAsync(
                    callbackQueryId: Context.Callback.Id,
                    text: "There are no prices buttons yet."
                );
                return;
            }

            await EditMessageAsync(
                text: !string.IsNullOrEmpty(type) && selectedValue != -1 ? MessageGenerators.GenerateSearchSettingsMessage(userSettings) : null,
                replyMarkup: prices,
                parseMode: ParseMode.Markdown
            );
        }

        [Callback(CallbackDataTypes.UpdateFloors)]
        public async Task HandleUpdateFloorsAsync(string type = null, int selectedValue = -1)
        {
            var userSettings = await _dbContext.UserSettings
                .AsSplitQuery()
                .Include(us => us.SelectedCategories)
                .Include(us => us.SelectedRegions)
                .Include(us => us.Rooms)
                .Select(us => us)
                .FirstOrDefaultAsync(us => us.User.TelegramAccountId == Context.User.Id);

            if (userSettings == null)
            {
                return;
            }


            if (selectedValue != -1 && !string.IsNullOrEmpty(type))
            {
                switch (type)
                {
                    case KeyboardGenerator.MaxOperation:
                        userSettings.MaxFloor = userSettings.MaxFloor == selectedValue ? null : selectedValue;
                        break;

                    case KeyboardGenerator.MinOperation:
                        userSettings.MinFloor = userSettings.MinFloor == selectedValue ? null : selectedValue;
                        break;

                    default: break;
                }

                await _dbContext.SaveChangesAsync();
            }

            var floors = await _keyboards.InlineSearchAsync(CallbackDataTypes.UpdateFloors, userSettings);

            if (!floors.InlineKeyboard.Any())
            {
                await Context.BotService.Client.AnswerCallbackQueryAsync(
                    callbackQueryId: Context.Callback.Id,
                    text: "There are no floors buttons yet."
                );
                return;
            }

            await EditMessageAsync(
                text: !string.IsNullOrEmpty(type) && selectedValue != -1 ? MessageGenerators.GenerateSearchSettingsMessage(userSettings) : null,
                replyMarkup: floors,
                parseMode: ParseMode.Markdown
            );
        }

        [Callback(CallbackDataTypes.UpdateCategories)]
        public async Task HandleUpdateCategoriesAsync(int categoryId = -1)
        {
            var userSettings = await _dbContext.UserSettings
                .AsSplitQuery()
                .Include(us => us.SelectedCategories)
                .Include(us => us.SelectedRegions)
                .Include(us => us.Rooms)
                .Select(us => us)
                .FirstOrDefaultAsync(us => us.User.TelegramAccountId == Context.User.Id);

            if (userSettings == null)
            {
                return;
            }

            if (categoryId != -1)
            {
                var selectedCategory = userSettings.SelectedCategories.FirstOrDefault(c => c.Id == categoryId);

                if (selectedCategory == null)
                {
                    userSettings.SelectedCategories.Add(
                        _dbContext.Categories.First(c => c.Id == categoryId)
                    );
                }
                else
                {
                    userSettings.SelectedCategories.Remove(selectedCategory);
                }
                await _dbContext.SaveChangesAsync();
            }

            var categories = await _keyboards.InlineSearchAsync(
                CallbackDataTypes.UpdateCategories, userSettings
            );

            if (!categories.InlineKeyboard.Any())
            {
                await Context.BotService.Client.AnswerCallbackQueryAsync(
                    callbackQueryId: Context.Callback.Id,
                    text: "There are no categories yet."
                );
                return;
            }

            await EditMessageAsync(
                text: categoryId != -1 ? MessageGenerators.GenerateSearchSettingsMessage(userSettings) : null,
                replyMarkup: categories,
                parseMode: ParseMode.Markdown
            );
        }

        [Callback(CallbackDataTypes.UpdateRegions)]
        public async Task HandleUpdateRegionsAsync(int regionId = -1)
        {
            var userSettings = await _dbContext.UserSettings
                .AsSplitQuery()
                .Include(us => us.SelectedCategories)
                .Include(us => us.SelectedRegions)
                .Include(us => us.Rooms)
                .Select(us => us)
                .FirstOrDefaultAsync(us => us.User.TelegramAccountId == Context.User.Id);

            if (userSettings == null)
            {
                return;
            }

            if (regionId != -1)
            {
                var selectedRegion = userSettings.SelectedRegions.FirstOrDefault(c => c.Id == regionId);

                if (selectedRegion == null)
                {
                    userSettings.SelectedRegions.Add(
                        _dbContext.DirectoryItems.First(c => c.Id == regionId)
                    );
                }
                else
                {
                    userSettings.SelectedRegions.Remove(selectedRegion);
                }
                await _dbContext.SaveChangesAsync();
            }

            var regions = await _keyboards.InlineSearchAsync(
                CallbackDataTypes.UpdateRegions, userSettings
            );

            if (!regions.InlineKeyboard.Any())
            {
                await Context.BotService.Client.AnswerCallbackQueryAsync(
                    callbackQueryId: Context.Callback.Id,
                    text: "There are no regions yet."
                );
                return;
            }

            await EditMessageAsync(
                text: regionId != -1 ? MessageGenerators.GenerateSearchSettingsMessage(userSettings) : null,
                replyMarkup: regions,
                parseMode: ParseMode.Markdown
            );
        }
    }
}