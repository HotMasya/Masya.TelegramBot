namespace Masya.TelegramBot.Commands.Options
{
    public sealed class CommandServiceOptions
    {
        public char Prefix { get; set; } = '/';
        public char ArgsSeparator { get; set; } = ' ';
        public int StepCommandTimeout { get; set; } = 30;
    }
}