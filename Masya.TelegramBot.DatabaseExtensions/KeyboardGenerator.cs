using System;
using System.Collections.Generic;
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

        private async Task<InlineKeyboardMarkup> ChangeCategoriesAsync()
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var categories = await dbContext.Categories.ToListAsync();
            var rows = (int)Math.Ceiling(categories.Count / (double)Options.MaxSearchColumns) + 1;
            InlineKeyboardButton[][] buttons = new InlineKeyboardButton[rows][];
            var categoriesIndex = 0;
            for (int i = 0; i < rows - 1; i++)
            {
                for (int j = 0; j < Options.MaxSearchColumns; j++)
                {
                    if (categoriesIndex == categories.Count) break;
                    buttons[i] = new InlineKeyboardButton[Options.MaxSearchColumns];
                    buttons[i][j] = InlineKeyboardButton.WithCallbackData(
                        categories[categoriesIndex].Name,
                        string.Join(
                            Options.CallbackDataSeparator,
                            CallbackDataTypes.UpdateCategories,
                            categories[categoriesIndex].Id.ToString()
                        )
                    );
                    categoriesIndex++;
                }
                if (categoriesIndex == categories.Count) break;
            }

            buttons[rows - 1] = new InlineKeyboardButton[1];
            buttons[rows - 1][0] = InlineKeyboardButton.WithCallbackData("⬅ Go back", CallbackDataTypes.SearchMenu);
            return new InlineKeyboardMarkup(buttons);
        }

        private async Task<InlineKeyboardMarkup> ChangeSettingByTypeAsync(DirectoryType type)
        {
            using var scope = Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var regions = await dbContext.DirectoryItems
                .Where(di => di.DirectoryId == (int)type)
                .ToListAsync();

            if (regions.Count == 0)
            {
                return null;
            }
            var rows = (int)Math.Ceiling(regions.Count / (double)Options.MaxSearchColumns) + 1;
            var buttons = new List<List<InlineKeyboardButton>>(rows);
            var regionsIndex = 0;
            for (int i = 0; i < rows - 1; i++)
            {
                for (int j = 0; j < Options.MaxSearchColumns; j++)
                {
                    if (regionsIndex == regions.Count) break;
                    buttons.Add(new List<InlineKeyboardButton>(Options.MaxSearchColumns));
                    buttons[i][j] = InlineKeyboardButton.WithCallbackData(
                        regions[regionsIndex].Value,
                        string.Join(
                            Options.CallbackDataSeparator,
                            CallbackDataTypes.UpdateRegions,
                            regions[regionsIndex].Id.ToString()
                        )
                    );
                    regionsIndex++;
                }
                if (regionsIndex == regions.Count) break;
            }
            buttons.Add(new List<InlineKeyboardButton>(1));
            buttons[rows - 1][0] = InlineKeyboardButton.WithCallbackData("⬅ Go back", CallbackDataTypes.SearchMenu);
            return new InlineKeyboardMarkup(buttons);
        }

        public async Task<InlineKeyboardMarkup> InlineSearchAsync(string callbackDataType = null)
        {
            return callbackDataType switch
            {
                CallbackDataTypes.UpdateRegions => await ChangeSettingByTypeAsync(DirectoryType.District),
                CallbackDataTypes.UpdateCategories => await ChangeCategoriesAsync(),
                CallbackDataTypes.ChangeSettings => new InlineKeyboardMarkup(
                    new InlineKeyboardButton[][] {
                        new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("🏡Categories", CallbackDataTypes.UpdateCategories),
                            InlineKeyboardButton.WithCallbackData("🔍Regions", CallbackDataTypes.UpdateRegions),
                        },
                        new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("🚪Rooms", CallbackDataTypes.UpdateFloors),
                            InlineKeyboardButton.WithCallbackData("💵Price", CallbackDataTypes.UpdatePrice),
                            InlineKeyboardButton.WithCallbackData("🏢Floors", CallbackDataTypes.UpdateFloors)
                        }
                    }
                ),
                CallbackDataTypes.SearchMenu => new InlineKeyboardMarkup(
                    new InlineKeyboardButton[]{
                        InlineKeyboardButton.WithCallbackData("🔍Search", CallbackDataTypes.ExecuteSearch),
                        InlineKeyboardButton.WithCallbackData("⚙Settings", CallbackDataTypes.ChangeSettings)
                    }
                ),
                _ => null
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