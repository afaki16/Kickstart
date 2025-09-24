using {{PROJECT_NAME}}.Application.DTOs.Auth;
using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;

namespace {{PROJECT_NAME}}.Application.Features.Auth.Commands
{
    public class RefreshTokenCommand : IRequest<Result<LoginResponseDto>>
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }
} 
