using Kickstart.Application.Interfaces;
using Kickstart.Application.Features.Auth.Commands.Login;
using Kickstart.Application.Features.Auth.Commands.Logout;
using Kickstart.Application.Features.Auth.Commands.LogoutAll;
using Kickstart.Application.Features.Auth.Commands.LogoutDevice;
using Kickstart.Application.Features.Auth.Commands.Register;
using Kickstart.Application.Features.Auth.Commands.RefreshToken;
using Kickstart.Application.Features.Auth.Commands.RevokeSession;
using Kickstart.Application.Features.Auth.Commands.ChangePassword;
using Kickstart.Application.Features.Auth.Commands.ForgotPassword;
using Kickstart.Application.Features.Auth.Commands.ResetPassword;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Application.Common.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Kickstart.Application.Features.Auth.Commands.RevokeSession
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
