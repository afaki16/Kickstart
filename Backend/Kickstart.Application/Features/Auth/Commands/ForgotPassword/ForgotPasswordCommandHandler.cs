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
            var user = await _unitOfWork.Users.GetByEmailAsync(normalizedEmail, request.TenantId);

            // Güvenlik: kullanıcı bulunamasa bile 200 dön
            if (user is null)
                return Result.Success();

            // Önceki aktif tokenları geçersiz kıl
            await _unitOfWork.PasswordResetTokens.InvalidatePreviousTokensAsync(user.Id);

            var plainToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            var hashedToken = RefreshTokenHasher.Hash(plainToken);

            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = hashedToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                RequestIpAddress = request.IpAddress
            };

            await _unitOfWork.PasswordResetTokens.AddAsync(resetToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var frontendUrl = _configuration["App:FrontendUrl"];
            var resetLink = $"{frontendUrl}/reset-password?token={HttpUtility.UrlEncode(plainToken)}&email={HttpUtility.UrlEncode(user.Email)}";

            await _emailService.SendPasswordResetAsync(user.Email, user.FirstName, resetLink);

            return Result.Success();
        }
    }
}
