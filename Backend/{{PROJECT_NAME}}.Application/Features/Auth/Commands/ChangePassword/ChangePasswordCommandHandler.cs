using {{PROJECT_NAME}}.Application.Interfaces;
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
using {{PROJECT_NAME}}.Domain.Common.Enums;
using {{PROJECT_NAME}}.Domain.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace {{PROJECT_NAME}}.Application.Features.Auth.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
{
    private readonly IPasswordService _passwordService;
    private readonly ICurrentUserService _currentUserService;

    public ChangePasswordCommandHandler(
        IPasswordService passwordService,
        ICurrentUserService currentUserService)
    {
        _passwordService = passwordService;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return Result<int>.Failure(Error.Failure(
               ErrorCode.NotFound,
               "User not authenticated"));

        // TODO: Implement password change logic
        return Result.Success();
    }
}
} 
