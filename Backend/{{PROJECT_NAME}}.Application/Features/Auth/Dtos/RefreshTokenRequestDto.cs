namespace {{PROJECT_NAME}}.Application.DTOs.Auth
{
    public class RefreshTokenRequestDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
} 
