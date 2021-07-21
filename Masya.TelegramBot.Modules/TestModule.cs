using Masya.TelegramBot.Commands;
using Masya.TelegramBot.Commands.Attributes;
using System;
using System.Text;
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
            
        [Command("testrem")]
        [Alias("tr")]
        public async Task RemainderCommandAsync(int count, [Remainder] params string[] names)
        {
            StringBuilder builder = new StringBuilder();
            foreach(string name in names)
            {
                builder.Append(name + "\n");
            }

            string result = string.Format("Count: <b>{0}</b>\nNames:\n<b>{1}</b>", count, builder.ToString());
            await ReplyAsync(result);
        }
    }
}
