using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Masya.TelegramBot.Commands.Options;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Models;
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

        public async Task<InlineKeyboardMarkup> InlineSearch(string callbackDataType = null)
        {
            using var scope = Services.CreateScope();
            switch (callbackDataType)
            {
                case CallbackDataTypes.UpdateCategories:
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var categories = await dbContext.Categories.ToListAsync();
                    var rows = (int)Math.Ceiling(categories.Count / (double)Options.MaxSearchColumns);
                    InlineKeyboardButton[][] buttons = new InlineKeyboardButton[rows][];
                    var categoriesIndex = 0;
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < Options.MaxSearchColumns; j++)
                        {
                            if (categoriesIndex == categories.Count) break;
                            buttons[i] = new InlineKeyboardButton[Options.MaxSearchColumns];
                            buttons[i][j] = new InlineKeyboardButton(categories[categoriesIndex].Name)
                            {
                                CallbackData = CallbackDataTypes.UpdateCategories + categories[categoriesIndex].Id
                            };
                            categoriesIndex++;
                        }
                        if (categoriesIndex == categories.Count) break;
                    }
                    return new InlineKeyboardMarkup(buttons);

                case CallbackDataTypes.ChangeSettings:
                    return new InlineKeyboardMarkup(
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
                    );

                default:
                    return new InlineKeyboardMarkup(
                        new InlineKeyboardButton[]{
                            InlineKeyboardButton.WithCallbackData("🔍Search", CallbackDataTypes.ExecuteSearch),
                            InlineKeyboardButton.WithCallbackData("⚙Settings", CallbackDataTypes.ChangeSettings)
                        }
                    );
            }
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