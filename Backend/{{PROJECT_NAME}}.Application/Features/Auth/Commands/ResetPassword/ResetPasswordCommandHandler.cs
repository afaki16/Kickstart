using {{PROJECT_NAME}}.Application.Features.Auth.Commands;
using {{PROJECT_NAME}}.Application.Services;
using {{PROJECT_NAME}}.Application.Common.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace {{PROJECT_NAME}}.Application.Features.Auth.Handlers
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
    {
        private readonly IPasswordService _passwordService;

        public ResetPasswordCommandHandler(IPasswordService passwordService)
        {
            _passwordService = passwordService;
        }

        public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            // TODO: Implement password reset logic
            return Result.Success();
        }
    }
} 
