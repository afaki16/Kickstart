using {{PROJECT_NAME}}.Application.Features.Auth.Commands;
using {{PROJECT_NAME}}.Application.Services;
using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;


namespace {{PROJECT_NAME}}.Application.Features.Auth.Handlers
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
    {
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        public ForgotPasswordCommandHandler(IUserService userService, IEmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
        }

        public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            // TODO: Implement forgot password logic
            return Result.Success();
        }
    }
} 
