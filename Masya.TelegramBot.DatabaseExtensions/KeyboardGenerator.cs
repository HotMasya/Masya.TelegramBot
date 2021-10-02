using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Options;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Models;
using Masya.TelegramBot.DataAccess.Types;
using Masya.TelegramBot.DatabaseExtensions.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types.ReplyMarkups;

namespace Masya.TelegramBot.DatabaseExtensions
{
    public sealed class KeyboardGenerator : IKeyboardGenerator
    {
        public IServiceProvider Services { get; }
        public CommandServiceOptions Options { get; }

        public const string MaxOperation = "__max__";
        public const string MinOperation = "__min__";

        public KeyboardGenerator(IServiceProvider services, IOptions<CommandServiceOptions> options)
        {
            Services = services;
            Options = options.Value;
        }

        private void UpdateButtons(List<List<KeyboardButton>> buttons, ref int currentRowIndex)
        {
            if (buttons.Count == 0)
            {
                buttons.Add(new List<KeyboardButton>());
            }
            else if (buttons[currentRowIndex].Count == Options.MaxMenuColumns)
            {
                currentRowIndex++;
                buttons.Add(new List<KeyboardButton>());
            }
        }

        private async Task<InlineKeyboardMarkup> ChangeCategoriesAsync(IEnumerable<Category> selectedCategories)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var categories = await dbContext.Categories.ToListAsync();
            var rows = (int)Math.Ceiling(categories.Count / (double)Options.MaxSearchColumns) + 1;
            var buttons = new List<List<InlineKeyboardButton>>();
            var categoriesIndex = 0;
            for (int i = 0; i < rows - 1; i++)
            {
                buttons.Add(new List<InlineKeyboardButton>());
                for (int j = 0; j < Options.MaxSearchColumns && categoriesIndex < categories.Count; j++, categoriesIndex++)
                {
                    buttons[^1].Add(
                        InlineKeyboardButton.WithCallbackData(
                            string.Format(
                                "{0} {1}",
                                selectedCategories.Any(sc => sc.Id == categories[categoriesIndex].Id) ? "✅" : "",
                                categories[categoriesIndex].Name
                            ),
                            string.Join(
                                Options.CallbackDataSeparator,
                                CallbackDataTypes.UpdateCategories,
                                categories[categoriesIndex].Id.ToString()
                            )
                        )
                    );
                }
                if (categoriesIndex == categories.Count) break;
            }
            buttons.Add(new List<InlineKeyboardButton>(){
                InlineKeyboardButton.WithCallbackData("⬅ Go back", CallbackDataTypes.ChangeSettings)
            });
            return new InlineKeyboardMarkup(buttons);
        }

        private async Task<InlineKeyboardMarkup> ChangeRoomsAsync(IEnumerable<Room> selectedRooms)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var rooms = await dbContext.Rooms.ToListAsync();

            if (rooms.Count == 0)
            {
                return null;
            }
            var buttons = new List<List<InlineKeyboardButton>>();
            foreach (var room in rooms)
            {
                buttons.Add(new List<InlineKeyboardButton>(){
                    InlineKeyboardButton.WithCallbackData(
                        string.Format(
                            "{0} {1}",
                            selectedRooms.Any(sr => sr.Id == room.Id) ? "✅" : "",
                            room.RoomsCount
                        ),
                        string.Join(
                            Options.CallbackDataSeparator,
                            CallbackDataTypes.UpdateRooms,
                            room.Id
                        )
                    )
                });
            }
            buttons.Add(new List<InlineKeyboardButton>(){
                InlineKeyboardButton.WithCallbackData("⬅ Go back", CallbackDataTypes.ChangeSettings)
            });
            return new InlineKeyboardMarkup(buttons);
        }

        private async Task<InlineKeyboardMarkup> ChangeSettingByTypeAsync(DirectoryType type, IEnumerable<DirectoryItem> selectedRegions)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var regions = await dbContext.DirectoryItems
                .AsQueryable()
                .Where(di => di.DirectoryId == (int)type)
                .OrderBy(di => di.Value)
                .ToListAsync();

            if (regions.Count == 0)
            {
                return null;
            }
            var rows = (int)Math.Ceiling(regions.Count / (double)Options.MaxSearchColumns) + 1;
            var buttons = new List<List<InlineKeyboardButton>>();
            var regionsIndex = 0;
            for (int i = 0; i < rows - 1; i++)
            {
                buttons.Add(new List<InlineKeyboardButton>());
                for (int j = 0; j < Options.MaxSearchColumns && regionsIndex < regions.Count; j++, regionsIndex++)
                {
                    buttons[^1].Add(
                        InlineKeyboardButton.WithCallbackData(
                            string.Format(
                                "{0} {1}",
                                selectedRegions.Any(sc => sc.Id == regions[regionsIndex].Id) ? "✅" : "",
                                regions[regionsIndex].Value
                            ),
                            string.Join(
                                Options.CallbackDataSeparator,
                                CallbackDataTypes.UpdateRegions,
                                regions[regionsIndex].Id.ToString()
                            )
                        )
                    );
                }
                if (regionsIndex == regions.Count) break;
            }
            buttons.Add(new List<InlineKeyboardButton>(){
                InlineKeyboardButton.WithCallbackData("⬅ Go back", CallbackDataTypes.ChangeSettings)
            });
            return new InlineKeyboardMarkup(buttons);
        }

        private async Task<InlineKeyboardMarkup> ChangePriceAsync(int? selectedMinPrice, int? selectedMaxPrice)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var prices = await dbContext.Prices.ToListAsync();
            if (prices.Count == 0)
            {
                return null;
            }
            var buttons = new List<List<InlineKeyboardButton>>();
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            foreach (var price in prices)
            {
                buttons.Add(new List<InlineKeyboardButton>{
                    InlineKeyboardButton.WithCallbackData(
                        string.Format(
                            "{0} {1}",
                            selectedMinPrice.HasValue && price.MinVal.Equals(selectedMinPrice.Value) ? "✅" : "",
                            price.MinVal.ToString("#,0", nfi)
                        ),
                        string.Join(
                            Options.CallbackDataSeparator,
                            CallbackDataTypes.UpdatePrice,
                            MinOperation,
                            price.MinVal.ToString()
                        )
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        string.Format(
                            "{0} {1}",
                            selectedMaxPrice.HasValue && price.MaxVal.Equals(selectedMaxPrice.Value) ? "✅" : "",
                            price.MaxVal.ToString("#,0", nfi)
                        ),
                        string.Join(
                            Options.CallbackDataSeparator,
                            CallbackDataTypes.UpdatePrice,
                            MaxOperation,
                            price.MaxVal.ToString()
                        )
                    )
                });
            }
            buttons.Add(new List<InlineKeyboardButton>(){
                InlineKeyboardButton.WithCallbackData("⬅ Go back", CallbackDataTypes.ChangeSettings)
            });
            return new InlineKeyboardMarkup(buttons);
        }

        private async Task<InlineKeyboardMarkup> ChangeFloorsAsync(int? selectedMinFloor, int? selectedMaxFloor)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var floors = await dbContext.Floors.ToListAsync();
            if (floors.Count == 0)
            {
                return null;
            }
            var buttons = new List<List<InlineKeyboardButton>>();
            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";
            foreach (var floor in floors)
            {
                buttons.Add(new List<InlineKeyboardButton>{
                    InlineKeyboardButton.WithCallbackData(
                        string.Format(
                            "{0} {1}",
                            selectedMinFloor.HasValue && floor.MinVal.Equals(selectedMinFloor.Value) ? "✅" : "",
                            floor.MinVal.ToString("#,0", nfi)
                        ),
                        string.Join(
                            Options.CallbackDataSeparator,
                            CallbackDataTypes.UpdateFloors,
                            MinOperation,
                            floor.MinVal.ToString()
                        )
                    ),
                    InlineKeyboardButton.WithCallbackData(
                        string.Format(
                            "{0} {1}",
                            selectedMaxFloor.HasValue && floor.MaxVal.Equals(selectedMaxFloor.Value) ? "✅" : "",
                            floor.MaxVal.ToString("#,0", nfi)
                        ),
                        string.Join(
                            Options.CallbackDataSeparator,
                            CallbackDataTypes.UpdateFloors,
                            MaxOperation,
                            floor.MaxVal.ToString()
                        )
                    )
                });
            }
            buttons.Add(new List<InlineKeyboardButton>(){
                InlineKeyboardButton.WithCallbackData("⬅ Go back", CallbackDataTypes.ChangeSettings)
            });
            return new InlineKeyboardMarkup(buttons);
        }

        public async Task<InlineKeyboardMarkup> InlineSearchAsync(string callbackDataType = null, UserSettings userSettings = null)
        {
            return callbackDataType switch
            {
                CallbackDataTypes.UpdateRegions => await ChangeSettingByTypeAsync(DirectoryType.District, userSettings.SelectedRegions),
                CallbackDataTypes.UpdateCategories => await ChangeCategoriesAsync(userSettings.SelectedCategories),
                CallbackDataTypes.UpdatePrice => await ChangePriceAsync(userSettings.MinPrice, userSettings.MaxPrice),
                CallbackDataTypes.UpdateFloors => await ChangeFloorsAsync(userSettings.MinFloor, userSettings.MaxFloor),
                CallbackDataTypes.UpdateRooms => await ChangeRoomsAsync(userSettings.Rooms),
                CallbackDataTypes.ChangeSettings => new InlineKeyboardMarkup(
                    new InlineKeyboardButton[][] {
                        new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("🏡Categories", CallbackDataTypes.UpdateCategories),
                            InlineKeyboardButton.WithCallbackData("🔍Regions", CallbackDataTypes.UpdateRegions),
                        },
                        new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("🚪Rooms", CallbackDataTypes.UpdateRooms),
                            InlineKeyboardButton.WithCallbackData("💵Price", CallbackDataTypes.UpdatePrice),
                            InlineKeyboardButton.WithCallbackData("🏢Floors", CallbackDataTypes.UpdateFloors)
                        },
                        new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("⬅ Go back", "_go_back_")
                        }
                    }
                ),
                CallbackDataTypes.MainMenu => new InlineKeyboardMarkup(
                    new InlineKeyboardButton[]{
                        InlineKeyboardButton.WithCallbackData("🔍Search", CallbackDataTypes.ExecuteSearch),
                        InlineKeyboardButton.WithCallbackData("⚙Settings", CallbackDataTypes.ChangeSettings)
                    }
                ),
                _ => null,
            };
        }

        public IReplyMarkup Menu(Permission userPermission)
        {
            var buttons = new List<List<KeyboardButton>>();
            int currentRowIndex = 0;
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var commands = dbContext.Commands
                .AsQueryable()
                .Where(c => c.ParentCommand == null)
                .Include(c => c.Aliases)
                .ToList();

            if (commands.Count == 0)
            {
                return new ReplyKeyboardRemove();
            }

            foreach (var c in commands)
            {
                if (c.DisplayInMenu && userPermission >= c.Permission)
                {
                    UpdateButtons(buttons, ref currentRowIndex);
                    buttons[currentRowIndex].Add(new KeyboardButton(c.Name));
                }

                if (c.Aliases is not null && c.Aliases.Count > 0)
                {
                    foreach (var a in c.Aliases)
                    {
                        if (a.DisplayInMenu && userPermission >= a.Permission)
                        {
                            UpdateButtons(buttons, ref currentRowIndex);
                            buttons[currentRowIndex].Add(new KeyboardButton(a.Name));
                        }
                    }
                }
            }
            return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
        }

        public IReplyMarkup Roles()
        {
            var buttons = new List<KeyboardButton>
            {
                new KeyboardButton("Agent"),
                new KeyboardButton("Customer")
            };
            return new ReplyKeyboardMarkup(buttons);
        }
    }
}