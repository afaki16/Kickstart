using Kickstart.Application.Common.Results;
using Kickstart.Application.Common.Security;
using Kickstart.Application.Interfaces;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Web;

namespace Kickstart.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public ForgotPasswordCommandHandler(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email?.Trim().ToLowerInvariant();

            // Mirror the login flow: look up across tenants, then narrow only when ambiguous.
            // GetByEmailAsync(email, tenantId) misses TenantId=null users when request.TenantId == 0.
            var users = await _unitOfWork.Users.GetUsersByEmailAsync(normalizedEmail);

            User? targetUser = null;
            if (users.Count == 1)
            {
                targetUser = users[0];
            }
            else if (users.Count > 1 && request.TenantId > 0)
            {
                targetUser = users.FirstOrDefault(u => u.TenantId == request.TenantId);
            }
            // users.Count == 0 OR (multiple users but no tenant disambiguator):
            // fall through with targetUser == null. Returning Success either way avoids
            // leaking whether the email exists / which tenant it belongs to.

            if (targetUser is null)
                return Result.Success();

            // Önceki aktif tokenları geçersiz kıl
            await _unitOfWork.PasswordResetTokens.InvalidatePreviousTokensAsync(targetUser.Id);

            var plainToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var hashedToken = RefreshTokenHasher.Hash(plainToken);

            var resetToken = new PasswordResetToken
            {
                UserId = targetUser.Id,
                Token = hashedToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                RequestIpAddress = request.IpAddress
            };

            await _unitOfWork.PasswordResetTokens.AddAsync(resetToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var frontendUrl = _configuration["App:FrontendUrl"];
            var resetLink = $"{frontendUrl}/reset-password?token={HttpUtility.UrlEncode(plainToken)}&email={HttpUtility.UrlEncode(targetUser.Email)}";

            await _emailService.SendPasswordResetAsync(targetUser.Email, targetUser.FirstName, resetLink);

            return Result.Success();
        }
    }
}
