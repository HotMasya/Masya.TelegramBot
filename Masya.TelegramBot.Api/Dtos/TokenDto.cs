namespace Masya.TelegramBot.Api.Dtos
{
    public sealed class TokenDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public TokenDto() { }

        public TokenDto(string accessToken)
        {
            AccessToken = accessToken;
        }

        public TokenDto(string accessToken, string refreshToken)
            : this(accessToken)
        {
            RefreshToken = refreshToken;
        }
    }
}