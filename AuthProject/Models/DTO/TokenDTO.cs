namespace AuthProject.Models.DTO
{
    public class TokenDTO
    {
        public bool Authenticated { get; set; }
        public string CreatedDate { get; set; }
        public string ExpirationDate { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public TokenDTO(bool authenticated, string createdDate, string expirationDate, string accessToken, string refreshToken)
        {
            this.Authenticated = authenticated;
            this.CreatedDate = createdDate;
            this.ExpirationDate = expirationDate;
            this.AccessToken = accessToken;
            this.RefreshToken = refreshToken;
            
        }
    }
}