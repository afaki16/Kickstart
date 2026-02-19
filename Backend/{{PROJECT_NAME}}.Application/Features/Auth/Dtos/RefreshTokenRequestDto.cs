namespace {{PROJECT_NAME}}.Application.Features.Auth.Dtos
{
    public class RefreshTokenRequestDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
} 
