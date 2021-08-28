using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace Masya.TelegramBot.Modules
{
    public static class Markups
    {
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
