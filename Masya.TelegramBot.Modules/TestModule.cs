using Masya.TelegramBot.Commands;
using Masya.TelegramBot.Commands.Attributes;
using System;
using System.Threading.Tasks;

namespace Masya.TelegramBot.Modules
{
    public class TestModule : Module
    {
        [Command("testtime")]
        [Alias("tt")]
        public async Task TestCommandAsync([ParseFormat("%s")] TimeSpan time)
        {
            await ReplyAsync("Вы указали время: " + time.ToString());
        }

        [Command("testdate")]
        [Alias("td")]
        public async Task TestDateCommandAsync(DateTime date)
        {
            await ReplyAsync("Вы указали дату: " + date.ToString("dd.MM.yyyy"));
        }
    }
}
