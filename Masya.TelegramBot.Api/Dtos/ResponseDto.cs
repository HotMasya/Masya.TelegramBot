namespace Masya.TelegramBot.Api.Dtos
{
    public class ResponseDto<T> where T : class
    {
        public string Message { get; set; }
        public T Details { get; set; }

        public ResponseDto(string message)
        {
            Message = message;
            Details = default(T);
        }

        public ResponseDto(string message, T details)
        {
            Message = message;
        }
    }

    public sealed class MessageResponseDto : ResponseDto<object>
    {
        public MessageResponseDto(string message) : base(message) { }
    }
}