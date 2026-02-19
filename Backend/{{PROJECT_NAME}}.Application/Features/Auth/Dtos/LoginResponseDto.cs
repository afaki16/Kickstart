using System;
using {{PROJECT_NAME}}.Application.Features.Users.Dtos;

namespace {{PROJECT_NAME}}.Application.Features.Auth.Dtos
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public UserDto User { get; set; }
    }
} 
