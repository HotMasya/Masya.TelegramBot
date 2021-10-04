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

namespace Masya.TelegramBot.Modules
{
    public sealed class SearchCallbacksHandlerModule : DatabaseModule
    {
        private readonly IKeyboardGenerator _keyboards;
        private readonly ApplicationDbContext _dbContext;
        private readonly IDistributedCache _cache;
        private readonly ILogger<SearchCallbacksHandlerModule> _logger;

        private const string SearchProcessPrefix = "SearchProcess_";

        public SearchCallbacksHandlerModule(
            ApplicationDbContext dbContext,
            IKeyboardGenerator keyboards,
            ILogger<SearchCallbacksHandlerModule> logger,
            IDistributedCache cache
        )
        {
            _dbContext = dbContext;
            _keyboards = keyboards;
            _cache = cache;
            _logger = logger;
        }

        private async Task<DataAccess.Models.User> GetUserWithSettings()
        {
            return await _dbContext.Users
                .AsSplitQuery()
                .AsQueryable()
                .Include(u => u.UserSettings)
                    .ThenInclude(us => us.SelectedCategories)
                .Include(u => u.UserSettings)
                    .ThenInclude(us => us.SelectedRegions)
                .Include(u => u.UserSettings)
                    .ThenInclude(us => us.Rooms)
                .FirstOrDefaultAsync(u => u.TelegramAccountId == Context.User.Id);
        }

        [Command("/search")]
        public async Task SearchAsync()
        {
            var user = await GetUserWithSettings();

            if (user == null)
            {
                return;
            }

            if (user.UserSettings == null)
            {
                user.UserSettings = new UserSettings();
                await _dbContext.SaveChangesAsync();
            }

            await ReplyAsync(
                content: MessageGenerators.GenerateSearchSettingsMessage(user.UserSettings),
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
                    var results = await SearchRealtyObjectsAsync();

                    if (results.Count == 0)
                    {
                        await ReplyAsync(
                            "No result were found for your search.\n*Configure search settings:* /search",
                            ParseMode.Markdown
                        );
                        return;
                    }

                    if (results.Count > objLimit)
                    {
                        searchProcess = new SearchProcess
                        {
                            TelegramId = Context.User.Id,
                            RealtyObjects = results,
                            ItemsSentCount = objLimit,
                        };

                        await _cache.SetRecordAsync(
                            cacheKey,
                            searchProcess,
                            TimeSpan.FromMinutes(10),
                            TimeSpan.FromMinutes(3)
                        );

                        await SendResultsAsync(results.Take(searchProcess.ItemsSentCount).ToList());
                        await ReplyAsync(
                            content: string.Format("Sent *{0} of {1}* results.", searchProcess.ItemsSentCount, searchProcess.RealtyObjects.Count()),
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

                if (searchProcess.RealtyObjects.Count() > searchProcess.ItemsSentCount + objLimit)
                {
                    await _cache.RemoveAsync(cacheKey);

                    searchProcess.ItemsSentCount += objLimit;
                    searchProcess.RealtyObjects = searchProcess.RealtyObjects.Skip(objLimit);
                    await _cache.SetRecordAsync(
                        cacheKey,
                        searchProcess,
                        TimeSpan.FromMinutes(10),
                        TimeSpan.FromMinutes(3)
                    );

                    await SendResultsAsync(
                        searchProcess.RealtyObjects
                            .Take(searchProcess.ItemsSentCount)
                            .ToList()
                    );

                    await ReplyAsync(
                        content: string.Format("Sent *{0} of {1}* results.", searchProcess.ItemsSentCount, searchProcess.RealtyObjects.Count()),
                        replyMarkup: new InlineKeyboardMarkup(
                            InlineKeyboardButton.WithCallbackData("🔍See more", CallbackDataTypes.ExecuteSearch)
                        ),
                        parseMode: ParseMode.Markdown
                    );
                    return;
                }

                if (searchProcess.RealtyObjects.Count() <= searchProcess.ItemsSentCount + objLimit)
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

        private async Task SendResultsAsync(List<RealtyObject> results)
        {
            using var httpClient = new HttpClient();

            foreach (var r in results)
            {
                if (r.Images != null && r.Images.Count > 0)
                {
                    var photos = new List<InputMediaPhoto>
                        {
                            await UrlToTelegramPhotoAsync(
                                r.Images[0].Url,
                                r.Images[0].Id.ToString(),
                                httpClient,
                                BuildRealtyObjectDescr(r)
                            )
                        };

                    for (int i = 1; i < r.Images.Count; i++)
                    {
                        photos.Add(
                            await UrlToTelegramPhotoAsync(
                                r.Images[i].Url,
                                r.Images[i].Id.ToString(),
                                httpClient
                            )
                        );
                    }

                    await Context.BotService.Client.SendMediaGroupAsync(Context.Chat.Id, photos, true);
                }

                await ReplyAsync(BuildRealtyObjectDescr(r));
            }
        }

        private async Task<List<RealtyObject>> SearchRealtyObjectsAsync()
        {
            var user = await GetUserWithSettings();
            var userSettings = user.UserSettings;

            return await _dbContext.RealtyObjects
                .AsQueryable()
                    .Include(ro => ro.Images)
                    .Include(ro => ro.Category)
                    .Include(ro => ro.District)
                    .Include(ro => ro.WallMaterial)
                    .Include(ro => ro.State)
                    .Include(ro => ro.Street)
                .Where(
                    ro => userSettings.SelectedCategories.Any(sc => sc.Id == ro.CategoryId)
                    && userSettings.SelectedRegions.Any(sr => sr.Id == ro.DistrictId)
                    && (
                        !userSettings.MinPrice.HasValue
                        || ro.Price >= userSettings.MinPrice.Value
                    )
                    && (
                        !userSettings.MaxPrice.HasValue
                        || ro.Price <= userSettings.MaxPrice.Value
                    )
                    && (
                        !userSettings.MinFloor.HasValue
                        || ro.Floor >= userSettings.MinFloor.Value
                    )
                    && (
                        !userSettings.MaxFloor.HasValue
                        || ro.Floor <= userSettings.MaxFloor.Value
                    )
                    && userSettings.Rooms.Any(r => r.RoomsCount == ro.Floor)
                )
                .ToListAsync();
        }

        private static async Task<InputMediaPhoto> UrlToTelegramPhotoAsync(string url, string fileName, HttpClient client, string caption = null)
        {
            using var fImageStream = await client.GetStreamAsync(url);
            var inputFile = new InputMedia(fImageStream, fileName);
            var inputPhoto = new InputMediaPhoto(inputFile);
            if (!string.IsNullOrEmpty(caption))
            {
                inputPhoto.Caption = caption;
            }

            return inputPhoto;
        }

        private static string BuildRealtyObjectDescr(RealtyObject obj)
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(obj.Description))
            {
                builder.AppendLine(obj.Description);
            }

            if (!string.IsNullOrEmpty(obj.Phone))
            {
                builder.AppendLine(
                    string.Format("Contact(s): *{0}*", obj.Phone)
                );
            }

            return builder.ToString();
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
            var user = await GetUserWithSettings();
            if (selectedRoomsId != -1)
            {
                var selectedRooms = user.UserSettings.Rooms.FirstOrDefault(c => c.Id == selectedRoomsId);

                if (selectedRooms == null)
                {
                    user.UserSettings.Rooms.Add(
                        _dbContext.Rooms.First(c => c.Id == selectedRoomsId)
                    );
                }
                else
                {
                    user.UserSettings.Rooms.Remove(selectedRooms);
                }
                await _dbContext.SaveChangesAsync();
            }

            var rooms = await _keyboards.InlineSearchAsync(
                CallbackDataTypes.UpdateRooms, user.UserSettings
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
                text: selectedRoomsId != -1 ? MessageGenerators.GenerateSearchSettingsMessage(user.UserSettings) : null,
                replyMarkup: rooms,
                parseMode: ParseMode.Markdown
            );
        }

        [Callback(CallbackDataTypes.UpdatePrice)]
        public async Task HandleUpdatePriceAsync(string type = null, int selectedValue = -1)
        {
            var user = await GetUserWithSettings();

            if (selectedValue != -1 && !string.IsNullOrEmpty(type))
            {
                switch (type)
                {
                    case KeyboardGenerator.MaxOperation:
                        user.UserSettings.MaxPrice = user.UserSettings.MaxPrice == selectedValue ? null : selectedValue;
                        break;

                    case KeyboardGenerator.MinOperation:
                        user.UserSettings.MinPrice = user.UserSettings.MinPrice == selectedValue ? null : selectedValue;
                        break;

                    default:
                        break;
                }
                await _dbContext.SaveChangesAsync();
            }

            var prices = await _keyboards.InlineSearchAsync(CallbackDataTypes.UpdatePrice, user.UserSettings);

            if (!prices.InlineKeyboard.Any())
            {
                await Context.BotService.Client.AnswerCallbackQueryAsync(
                    callbackQueryId: Context.Callback.Id,
                    text: "There are no prices buttons yet."
                );
                return;
            }

            await EditMessageAsync(
                text: !string.IsNullOrEmpty(type) && selectedValue != -1 ? MessageGenerators.GenerateSearchSettingsMessage(user.UserSettings) : null,
                replyMarkup: prices,
                parseMode: ParseMode.Markdown
            );
        }

        [Callback(CallbackDataTypes.UpdateFloors)]
        public async Task HandleUpdateFloorsAsync(string type = null, int selectedValue = -1)
        {
            var user = await GetUserWithSettings();

            if (selectedValue != -1 && !string.IsNullOrEmpty(type))
            {
                switch (type)
                {
                    case KeyboardGenerator.MaxOperation:
                        user.UserSettings.MaxFloor = user.UserSettings.MaxFloor == selectedValue ? null : selectedValue;
                        break;

                    case KeyboardGenerator.MinOperation:
                        user.UserSettings.MinFloor = user.UserSettings.MinFloor == selectedValue ? null : selectedValue;
                        break;

                    default: break;
                }

                await _dbContext.SaveChangesAsync();
            }

            var floors = await _keyboards.InlineSearchAsync(CallbackDataTypes.UpdateFloors, user.UserSettings);

            if (!floors.InlineKeyboard.Any())
            {
                await Context.BotService.Client.AnswerCallbackQueryAsync(
                    callbackQueryId: Context.Callback.Id,
                    text: "There are no floors buttons yet."
                );
                return;
            }

            await EditMessageAsync(
                text: !string.IsNullOrEmpty(type) && selectedValue != -1 ? MessageGenerators.GenerateSearchSettingsMessage(user.UserSettings) : null,
                replyMarkup: floors,
                parseMode: ParseMode.Markdown
            );
        }

        [Callback(CallbackDataTypes.UpdateCategories)]
        public async Task HandleUpdateCategoriesAsync(int categoryId = -1)
        {
            var user = await GetUserWithSettings();

            if (user == null)
            {
                return;
            }

            if (categoryId != -1)
            {
                var selectedCategory = user.UserSettings.SelectedCategories.FirstOrDefault(c => c.Id == categoryId);

                if (selectedCategory == null)
                {
                    user.UserSettings.SelectedCategories.Add(
                        _dbContext.Categories.First(c => c.Id == categoryId)
                    );
                }
                else
                {
                    user.UserSettings.SelectedCategories.Remove(selectedCategory);
                }
                await _dbContext.SaveChangesAsync();
            }

            var categories = await _keyboards.InlineSearchAsync(
                CallbackDataTypes.UpdateCategories, user.UserSettings
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
                text: categoryId != -1 ? MessageGenerators.GenerateSearchSettingsMessage(user.UserSettings) : null,
                replyMarkup: categories,
                parseMode: ParseMode.Markdown
            );
        }

        [Callback(CallbackDataTypes.UpdateRegions)]
        public async Task HandleUpdateRegionsAsync(int regionId = -1)
        {
            var user = await GetUserWithSettings();

            if (user == null)
            {
                return;
            }

            if (regionId != -1)
            {
                var selectedRegion = user.UserSettings.SelectedRegions.FirstOrDefault(c => c.Id == regionId);

                if (selectedRegion == null)
                {
                    user.UserSettings.SelectedRegions.Add(
                        _dbContext.DirectoryItems.First(c => c.Id == regionId)
                    );
                }
                else
                {
                    user.UserSettings.SelectedRegions.Remove(selectedRegion);
                }
                await _dbContext.SaveChangesAsync();
            }

            var regions = await _keyboards.InlineSearchAsync(
                CallbackDataTypes.UpdateRegions, user.UserSettings
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
                text: regionId != -1 ? MessageGenerators.GenerateSearchSettingsMessage(user.UserSettings) : null,
                replyMarkup: regions,
                parseMode: ParseMode.Markdown
            );
        }
    }
}