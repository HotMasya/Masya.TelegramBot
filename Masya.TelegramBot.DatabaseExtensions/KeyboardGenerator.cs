using System;
using System.Collections.Generic;
using System.Linq;
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
                return new ReplyKeyboardMarkup();
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