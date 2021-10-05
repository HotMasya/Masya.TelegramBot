namespace Masya.TelegramBot.Commands.Options
{
    public sealed class CommandServiceOptions
    {
        public char ArgsSeparator { get; set; } = ' ';
        public int StepCommandTimeout { get; set; } = 30;
        public int MaxMenuColumns { get; set; } = 3;
        public int MaxSearchColumns { get; set; } = 3;
        public string CallbackDataSeparator { get; set; } = ";";

        //  Should be separated into the inherited class
        public int ObjectsSentLimit { get; set; } = 5;
    }
}