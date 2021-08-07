namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class TokenDto
    {
        public string Token { get; set; }

        public TokenDto(string token)
        {
            Token = token;
        }
    }
}