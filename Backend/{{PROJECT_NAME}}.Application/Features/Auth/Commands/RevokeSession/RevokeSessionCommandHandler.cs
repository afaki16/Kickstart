using {{PROJECT_NAME}}.Application.Features.Auth.Commands;
using {{PROJECT_NAME}}.Application.Services;
using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace {{PROJECT_NAME}}.Application.Features.Auth.Handlers
{
    public class RevokeSessionCommandHandler : IRequestHandler<RevokeSessionCommand, Result>
    {
        private readonly IAuthService _authService;

        public RevokeSessionCommandHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Result> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
        {
            return await _authService.RevokeTokenAsync(request.RefreshToken, request.IpAddress, request.UserAgent, request.Reason);
        }
    }
} 
