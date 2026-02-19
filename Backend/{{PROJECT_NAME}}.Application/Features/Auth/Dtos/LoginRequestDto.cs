namespace {{PROJECT_NAME}}.Application.Features.Auth.Dtos
{
    public class LoginRequestDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
    }
} 
