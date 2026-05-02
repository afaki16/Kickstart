using Kickstart.Application.Common.Results;
using Kickstart.Application.Common.Security;
using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Enums;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Models;
using MediatR;

namespace Kickstart.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;

        public ResetPasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordService passwordService)
        {
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
        }

        public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email?.Trim().ToLowerInvariant();
            var hashedToken = RefreshTokenHasher.Hash(request.Token);
            var resetToken = await _unitOfWork.PasswordResetTokens.GetValidTokenAsync(hashedToken, normalizedEmail);

            if (resetToken is null)
                return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, "Geçersiz veya süresi dolmuş sıfırlama bağlantısı."));

            var strengthResult = _passwordService.ValidatePasswordStrength(request.NewPassword);
            if (!strengthResult.IsSuccess)
                return Result.Failure(strengthResult.Errors);

            var hashResult = _passwordService.HashPassword(request.NewPassword);
            if (!hashResult.IsSuccess)
                return Result.Failure(hashResult.Errors);

            resetToken.User.PasswordHash = hashResult.Value;
            resetToken.User.LastSessionsRevokedAt = DateTime.UtcNow;
            resetToken.MarkAsUsed();

            _unitOfWork.Users.Update(resetToken.User);
            _unitOfWork.PasswordResetTokens.Update(resetToken);

            await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(
                resetToken.User.Id, reason: "Password reset");

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
