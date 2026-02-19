using {{PROJECT_NAME}}.Application.Features.Auth.Dtos;
using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;

namespace {{PROJECT_NAME}}.Application.Features.Auth.Commands.Login
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
