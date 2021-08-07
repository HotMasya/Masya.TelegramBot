namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class ResponseDto<T> where T : class
    {
        public string Message { get; set; }
        public T Details { get; set; }

        public ResponseDto(string message)
        {
            Message = message;
        }
    }
}