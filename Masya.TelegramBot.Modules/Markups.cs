using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace Masya.TelegramBot.Modules
{
    public static class Markups
    {
        public static IReplyMarkup MainMenuButtons()
        {
            var buttons = new List<List<KeyboardButton>>
            {
                new List<KeyboardButton>
                {
                    new KeyboardButton("СТАРТ"),
                    new KeyboardButton("СПРАВКА"),
                    new KeyboardButton("ОБЪЕКТ"),
                },
                new List<KeyboardButton>
                {
                    new KeyboardButton("ПОИСК"),
                    new KeyboardButton("МОИ"),
                    new KeyboardButton("РАССЫЛКА"),
                },
                new List<KeyboardButton>
                {
                    new KeyboardButton("ДОБАВИТЬ"),
                    new KeyboardButton("ЭКСПОРТ"),
                    new KeyboardButton("РЕГИСТРАЦИЯ")
                }
            };

            return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
        }

        public static IReplyMarkup RegisterButton()
        {
            return new ReplyKeyboardMarkup(KeyboardButton.WithRequestContact("РЕГИСТРАЦИЯ"));
        }

        public static IReplyMarkup ClientAgentButtons()
        {
            var buttons = new List<KeyboardButton>
            {
                new KeyboardButton("Агент"),
                new KeyboardButton("Покупатель")
            };
            return new ReplyKeyboardMarkup(buttons);
        }
    }
}
