using {{PROJECT_NAME}}.Application.Features.Auth.Commands;
using {{PROJECT_NAME}}.Application.Services;
using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace {{PROJECT_NAME}}.Application.Features.Auth.Handlers
{
    public class LogoutDeviceCommandHandler : IRequestHandler<LogoutDeviceCommand, Result>
    {
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;

        public LogoutDeviceCommandHandler(IAuthService authService, ICurrentUserService currentUserService)
        {
            _authService = authService;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(LogoutDeviceCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                return Result.Failure("User not authenticated");

            return await _authService.RevokeTokensByDeviceAsync(userId.Value, request.DeviceId, request.IpAddress, request.UserAgent, request.Reason);
        }
    }
} 
