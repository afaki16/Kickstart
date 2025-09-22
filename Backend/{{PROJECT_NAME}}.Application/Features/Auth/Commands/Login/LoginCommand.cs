using {{PROJECT_NAME}}.Application.DTOs.Auth;
using {{PROJECT_NAME}}.Domain.Common;
using MediatR;

namespace {{PROJECT_NAME}}.Application.Features.Auth.Commands
{
    public class LoginCommand : IRequest<Result<LoginResponseDto>>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }
} 
