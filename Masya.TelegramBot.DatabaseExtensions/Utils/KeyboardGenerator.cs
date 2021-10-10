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
using Masya.TelegramBot.DatabaseExtensions.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types.ReplyMarkups;

namespace Masya.TelegramBot.DatabaseExtensions.Utils
{
    public sealed class KeyboardGenerator : IKeyboardGenerator
    {
        public CommandServiceOptions Options { get; }

        public const string MaxOperation = "__max__";
        public const string MinOperation = "__min__";

        private readonly ApplicationDbContext _dbContext;

        public KeyboardGenerator(
            IOptions<CommandServiceOptions> options,
            ApplicationDbContext dbContext
        )
        {
            Options = options.Value;
            _dbContext = dbContext;
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
            var categories = await _dbContext.Categories.ToListAsync();
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
            var rooms = await _dbContext.Rooms.ToListAsync();

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
            var regions = await _dbContext.DirectoryItems
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
            var prices = await _dbContext.Prices.ToListAsync();
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
            var floors = await _dbContext.Floors.ToListAsync();
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
                            InlineKeyboardButton.WithCallbackData("⬅ Go back", CallbackDataTypes.MainMenu)
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
            var commands = _dbContext.Commands
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

        public static IReplyMarkup Roles()
        {
            var buttons = new List<KeyboardButton>
            {
                new KeyboardButton("Agent"),
                new KeyboardButton("Customer")
            };
            return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
        }

        public async Task<InlineKeyboardMarkup> SelectCategoriesAsync()
        {
            var categories = await _dbContext.Categories.Select(c => new { c.Id, c.Name }).ToListAsync();
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
                            categories[categoriesIndex].Name,
                            string.Join(
                                Options.CallbackDataSeparator,
                                CallbackDataTypes.SetObjectType,
                                categories[categoriesIndex].Id.ToString()
                            )
                        )
                    );
                }
                if (categoriesIndex == categories.Count) break;
            }

            return new InlineKeyboardMarkup(buttons);
        }

        public async Task<InlineKeyboardMarkup> SearchStreetsResults(string query)
        {
            var streets = await _dbContext.DirectoryItems
                .AsQueryable()
                .Where(
                    di => di.DirectoryId == (int)DirectoryType.Street
                        && di.Value.Contains(query)
                )
                .ToListAsync();

            var rows = (int)Math.Ceiling(streets.Count / (double)Options.MaxSearchColumns) + 1;
            var buttons = new List<List<InlineKeyboardButton>>();
            var streetsIndex = 0;
            for (int i = 0; i < rows - 1; i++)
            {
                buttons.Add(new List<InlineKeyboardButton>());
                for (int j = 0; j < Options.MaxSearchColumns && streetsIndex < streets.Count; j++, streetsIndex++)
                {
                    buttons[^1].Add(
                        InlineKeyboardButton.WithCallbackData(
                            streets[streetsIndex].Value,
                            string.Join(
                                Options.CallbackDataSeparator,
                                CallbackDataTypes.SetObjectStreet,
                                streets[streetsIndex].Id.ToString()
                            )
                        )
                    );
                }
                if (streetsIndex == streets.Count) break;
            }

            buttons.Add(new List<InlineKeyboardButton>{
                InlineKeyboardButton.WithCallbackData("🔍 Search again", CallbackDataTypes.SetObjectStreet),
                InlineKeyboardButton.WithCallbackData("❌ Cancel", CallbackDataTypes.CancelSetObjectStreet)
            });

            return new InlineKeyboardMarkup(buttons);
        }

        public InlineKeyboardMarkup ShowCreationMenu(CreateProcess process)
        {
            if (!process.CategoryId.HasValue)
            {
                return null;
            }

            var buttons = new List<List<InlineKeyboardButton>>()
            {
                new List<InlineKeyboardButton>{
                    InlineKeyboardButton.WithCallbackData("Category", CallbackDataTypes.SetObjectType)
                },
                new List<InlineKeyboardButton>{
                    InlineKeyboardButton.WithCallbackData("Description", CallbackDataTypes.SetObjectDescription),
                    InlineKeyboardButton.WithCallbackData("Price", CallbackDataTypes.SetObjectPrice),
                },
                new List<InlineKeyboardButton>{
                    InlineKeyboardButton.WithCallbackData("Address", CallbackDataTypes.SetObjectStreet),
                    InlineKeyboardButton.WithCallbackData("District", CallbackDataTypes.SetObjectRegion),
                }
            };

            switch ((SuperType)process.CategoryId.Value)
            {
                case SuperType.Sector:
                    buttons.Add(new List<InlineKeyboardButton>
                    {
                        InlineKeyboardButton.WithCallbackData("Lot area", CallbackDataTypes.SetObjectLotArea),
                    });
                    break;

                case SuperType.Flat:
                case SuperType.Commercial:
                case SuperType.NewBuilding:
                case SuperType.Rental:
                    buttons.Add(new List<InlineKeyboardButton>
                    {
                        InlineKeyboardButton.WithCallbackData("Floor", CallbackDataTypes.SetObjectFloor),
                    });
                    break;

                default:
                    break;
            }

            switch ((SuperType)process.CategoryId.Value)
            {
                case SuperType.House:
                case SuperType.Flat:
                case SuperType.Commercial:
                case SuperType.NewBuilding:
                case SuperType.Rental:
                    buttons.Add(new List<InlineKeyboardButton>{
                        InlineKeyboardButton.WithCallbackData("Total Floors", CallbackDataTypes.SetObjectTotalFloors),
                    });
                    buttons.Add(new List<InlineKeyboardButton>
                    {
                        InlineKeyboardButton.WithCallbackData("Rooms", CallbackDataTypes.SetObjectRoomsCount),
                        InlineKeyboardButton.WithCallbackData("Kitchen Area", CallbackDataTypes.SetObjectKitchenArea),
                    });
                    buttons.Add(new List<InlineKeyboardButton>{
                        InlineKeyboardButton.WithCallbackData("Total Area", CallbackDataTypes.SetObjectTotalArea),
                        InlineKeyboardButton.WithCallbackData("Living Area", CallbackDataTypes.SetObjectLivingArea),
                    });
                    buttons.Add(new List<InlineKeyboardButton>{
                        InlineKeyboardButton.WithCallbackData("Wall Material", CallbackDataTypes.SetObjectWallsMaterial),
                        InlineKeyboardButton.WithCallbackData("State", CallbackDataTypes.SetObjectState),
                    });
                    break;

                default: break;
            }

            buttons.Add(new List<InlineKeyboardButton>{
                InlineKeyboardButton.WithCallbackData("❌ Cancel", CallbackDataTypes.CancelObjectCreation),
            });

            return new InlineKeyboardMarkup(buttons);
        }
    }
}