namespace Roxana_tema1.Models
{
    public class AuthenticateResponse
    {
        public string IdToken { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
