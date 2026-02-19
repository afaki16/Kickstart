using {{PROJECT_NAME}}.Application.Interfaces;
using AutoMapper;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.Login;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.Logout;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.LogoutAll;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.LogoutDevice;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.Register;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.RefreshToken;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.RevokeSession;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.ChangePassword;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.ForgotPassword;
using {{PROJECT_NAME}}.Application.Features.Auth.Commands.ResetPassword;
using {{PROJECT_NAME}}.Domain.Common.Interfaces;
using {{PROJECT_NAME}}.Domain.Common.Interfaces.Repositories;
using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace {{PROJECT_NAME}}.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<Application.Features.Auth.Dtos.LoginResponseDto>>
    {
        private readonly IAuthService _authService;

        public LoginCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Result<Application.Features.Auth.Dtos.LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            return await _authService.LoginAsync(request.Email, request.Password, request.IpAddress, request.UserAgent, request.DeviceId, request.DeviceName);
        }
    }
} 
