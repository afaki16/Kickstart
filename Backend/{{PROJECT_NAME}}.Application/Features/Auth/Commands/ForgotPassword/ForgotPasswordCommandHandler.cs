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
using MediatR;


namespace {{PROJECT_NAME}}.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
    {
        private readonly IEmailService _emailService;

        public ForgotPasswordCommandHandler(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            // TODO: Implement forgot password logic
            return Result.Success();
        }
    }
} 
