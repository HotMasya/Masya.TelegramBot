namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class TokenDto
    {
        public string AccessToken { get; set; }

        public TokenDto(string accessToken)
        {
            AccessToken = accessToken;
        }
    }
}