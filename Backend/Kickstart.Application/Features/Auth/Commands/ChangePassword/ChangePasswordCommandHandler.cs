using Kickstart.Application.Interfaces;
using Kickstart.Application.Common.Results;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Models;
using MediatR;

namespace Kickstart.Application.Features.Auth.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly ICurrentUserService _currentUserService;

        public ChangePasswordCommandHandler(
            IUnitOfWork unitOfWork,
            IPasswordService passwordService,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                return Result.Failure(Error.Failure(ErrorCode.Unauthorized, "User not authenticated"));

            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user is null)
                return Result.Failure(Error.Failure(ErrorCode.NotFound, "User not found"));

            var verifyResult = _passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash);
            if (!verifyResult.IsSuccess)
                return Result.Failure(verifyResult.Errors);

            if (!verifyResult.Value)
                return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, "Current password is incorrect"));

            var strengthResult = _passwordService.ValidatePasswordStrength(request.NewPassword);
            if (!strengthResult.IsSuccess)
                return Result.Failure(strengthResult.Errors);

            var hashResult = _passwordService.HashPassword(request.NewPassword);
            if (!hashResult.IsSuccess)
                return Result.Failure(hashResult.Errors);

            user.PasswordHash = hashResult.Value;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
