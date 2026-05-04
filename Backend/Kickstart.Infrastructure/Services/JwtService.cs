using Kickstart.Application.Interfaces;
using Kickstart.Application.Features.Auth.Dtos;
using Kickstart.Application.Features.Auth.Models;
using Kickstart.Application.Common.Security;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Application.Common.Results;
using Kickstart.Domain.Entities;
using Kickstart.Domain.Models;
using Kickstart.Domain.Common.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Kickstart.Infrastructure.Services
{
    public class JwtService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<JwtService> _logger;
        private readonly IBruteForceProtectionService _bruteForceService;
        private readonly IEmailService _emailService;

        public JwtService(
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            IPasswordService passwordService,
            ILogger<JwtService> logger,
            IBruteForceProtectionService bruteForceService,
            IEmailService emailService)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _logger = logger;
            _bruteForceService = bruteForceService;
            _emailService = emailService;
        }

    public async Task<Result<LoginResponseDto>> LoginAsync(string email, string password, string ipAddress, string userAgent, string deviceId = null, string deviceName = null, bool rememberMe = false, int? tenantId = null)
    {
        var normalizedEmail = email?.Trim().ToLowerInvariant() ?? string.Empty;
        var safeIp = ipAddress ?? string.Empty;

        try
        {
            var lockoutStatus = await _bruteForceService.CheckLockoutAsync(normalizedEmail, safeIp);
            if (lockoutStatus.IsLockedOut)
            {
                // Run dummy verify so locked-out path takes the same time as a real verify.
                _passwordService.VerifyPassword(password, SecurityConstants.DummyBCryptHash);

                await _bruteForceService.RecordFailedAttemptAsync(
                    normalizedEmail, safeIp, userAgent, "AccountLocked");

                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.Unauthorized,
                    "Invalid email or password"));
            }

            var candidates = await _unitOfWork.Users.GetUsersByEmailWithPermissionsAsync(normalizedEmail);

            User user = null;
            if (tenantId.HasValue && tenantId.Value > 0)
            {
                user = candidates.FirstOrDefault(u => u.TenantId == tenantId.Value);
            }
            else if (candidates.Count == 1)
            {
                user = candidates[0];
            }
            else if (candidates.Count > 1)
            {
                // Multiple tenants matched and caller did not disambiguate — dummy verify
                // before responding so the timing matches a real login attempt.
                _passwordService.VerifyPassword(password, SecurityConstants.DummyBCryptHash);

                await _bruteForceService.RecordFailedAttemptAsync(
                    normalizedEmail, safeIp, userAgent, "MultipleTenants");

                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.ValidationFailed,
                    "Multiple accounts with this email. Specify tenant id."));
            }

            if (user == null)
            {
                // User-not-found path runs the same BCrypt cost so an attacker cannot
                // distinguish missing accounts from real ones via response time.
                _passwordService.VerifyPassword(password, SecurityConstants.DummyBCryptHash);

                await RecordFailureAndNotifyAsync(normalizedEmail, safeIp, userAgent, "UserNotFound", null);

                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.Unauthorized,
                    "Invalid email or password"));
            }

            var passwordVerification = _passwordService.VerifyPassword(password, user.PasswordHash);
            if (!passwordVerification.IsSuccess || !passwordVerification.Value)
            {
                await RecordFailureAndNotifyAsync(normalizedEmail, safeIp, userAgent, "InvalidPassword", user);
                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.Unauthorized,
                    "Invalid email or password"));
            }

            if (user.Status != Domain.Common.Enums.UserStatus.Active)
            {
                await _bruteForceService.RecordFailedAttemptAsync(
                    normalizedEmail, safeIp, userAgent, "AccountInactive");

                _logger.LogWarning(
                    "Login attempt on inactive account. Email: {Email}, Status: {Status}, IP: {IpAddress}",
                    normalizedEmail, user.Status, safeIp);

                // Generic response — do not leak account-status info to the caller.
                // No email notification: the legitimate owner already knows the account is inactive.
                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.Unauthorized,
                    "Invalid email or password"));
            }

            var accessTokenResult = await GenerateAccessTokenAsync(user);
            if (!accessTokenResult.IsSuccess)
                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.InternalError,
                    "Failed to generate access token"));

            var refreshTokenResult = await GenerateRefreshTokenAsync(user, ipAddress, userAgent, deviceId, deviceName, rememberMe);
            if (!refreshTokenResult.IsSuccess)
                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.InternalError,
                    "Failed to generate refresh token"));

            await _bruteForceService.RecordSuccessfulAttemptAsync(normalizedEmail, safeIp, userAgent);

            user.LastLoginDate = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return Result<LoginResponseDto>.Success(
                BuildLoginResponse(user, accessTokenResult.Value, refreshTokenResult.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for email {Email}", normalizedEmail);
            return Result<LoginResponseDto>.Failure(Error.Failure(
                ErrorCode.InternalError,
                "An unexpected error occurred during login"));
        }
    }

    private async Task RecordFailureAndNotifyAsync(
        string email,
        string ipAddress,
        string userAgent,
        string failureReason,
        User user)
    {
        await _bruteForceService.RecordFailedAttemptAsync(email, ipAddress, userAgent, failureReason);

        if (user == null)
            return;

        var statusAfter = await _bruteForceService.CheckLockoutAsync(email, ipAddress);
        if (!IsNewTierTransition(statusAfter.FailureCount))
            return;

        try
        {
            await _emailService.SendBruteForceAlertAsync(
                user.Email,
                user.FirstName,
                ipAddress,
                statusAfter.FailureCount,
                statusAfter.LockoutMinutes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send brute force alert email to {Email}", email);
        }
    }

    private static bool IsNewTierTransition(int failureCount)
    {
        return failureCount == 5
            || failureCount == 10
            || failureCount == 15
            || failureCount == 20
            || failureCount == 25;
    }
    public async Task<Result<string>> GenerateAccessTokenAsync(User user)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim("firstName", user.FirstName),
                    new Claim("lastName", user.LastName),
                    new Claim("status", user.Status.ToString())
                };

                if (user.TenantId.HasValue)
                {
                    claims.Add(new Claim("tenant_id", user.TenantId.Value.ToString()));
                }

                // Add role claims
                var roles = user.UserRoles.Select(ur => ur.Role).ToList();
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                }


                var jwtSettings = _configuration.GetSection("JwtSettings");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var issuedAt = DateTime.UtcNow;
                claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(issuedAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64));

                var token = new JwtSecurityToken(
                    issuer: jwtSettings["Issuer"],
                    audience: jwtSettings["Audience"],
                    claims: claims,
                    notBefore: issuedAt,
                    expires: issuedAt.AddMinutes(int.Parse(jwtSettings["ExpiryInMinutes"])),
                    signingCredentials: credentials
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                return Result<string>.Success(tokenString);
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate access token for user {UserId}", user.Id);
                return Result<string>.Failure(Error.Failure(ErrorCode.InternalError, "Failed to generate access token"));
            }
    }

        public async Task<Result<RefreshTokenIssueResult>> GenerateRefreshTokenAsync(User user, string ipAddress, string userAgent, string deviceId = null, string deviceName = null, bool rememberMe = false, DateTime? preserveExpiryDate = null)
        {
            try
            {
                // Revoke existing active tokens for the same device if deviceId is provided
                if (!string.IsNullOrEmpty(deviceId))
                {
                    await _unitOfWork.RefreshTokens.RevokeTokensByDeviceAsync(user.Id, deviceId, ipAddress, userAgent, "New login from same device");
                }

                DateTime expiryDate;
                if (preserveExpiryDate.HasValue)
                {
                    expiryDate = preserveExpiryDate.Value;
                }
                else
                {
                    var expiryDays = rememberMe
                        ? int.Parse(_configuration["JwtSettings:RefreshTokenExpiryInDaysRememberMe"] ?? "30")
                        : int.Parse(_configuration["JwtSettings:RefreshTokenExpiryInDays"] ?? "7");
                    expiryDate = DateTime.UtcNow.AddDays(expiryDays);
                }

                var randomBytes = new byte[64];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomBytes);

                var plainToken = Convert.ToBase64String(randomBytes);
                var tokenHash = RefreshTokenHasher.Hash(plainToken);

                var refreshToken = new RefreshToken
                {
                    UserId = user.Id,
                    Token = tokenHash,
                    ExpiryDate = expiryDate,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    DeviceId = deviceId,
                    DeviceName = deviceName,
                    DeviceType = GetDeviceType(userAgent),
                    Location = await GetLocationFromIpAsync(ipAddress)
                };

                await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
                await _unitOfWork.SaveChangesAsync();

                return Result<RefreshTokenIssueResult>.Success(new RefreshTokenIssueResult
                {
                    Stored = refreshToken,
                    PlainToken = plainToken
                });
        }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate refresh token for user {UserId}", user.Id);
                return Result<RefreshTokenIssueResult>.Failure(Error.Failure(ErrorCode.InternalError, "Failed to generate refresh token"));
            }
    }

        private string GetDeviceType(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown";

            userAgent = userAgent.ToLower();
            
            if (userAgent.Contains("mobile") || userAgent.Contains("android") || userAgent.Contains("iphone") || userAgent.Contains("ipad"))
                return "Mobile";
            if (userAgent.Contains("tablet"))
                return "Tablet";
            if (userAgent.Contains("desktop") || userAgent.Contains("windows") || userAgent.Contains("mac") || userAgent.Contains("linux"))
                return "Desktop";
            
            return "Unknown";
        }

        private async Task<string> GetLocationFromIpAsync(string ipAddress)
        {
            // TODO: Implement IP geolocation service
            // For now, return a placeholder
            return "Unknown";
        }

    public async Task<Result<LoginResponseDto>> RefreshTokenAsync(string accessToken, string refreshToken, string ipAddress, string userAgent)
    {
        try
        {
            var principal = GetClaimsFromExpiredToken(accessToken);
            if (principal == null)
                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.InvalidRequest,
                    "Invalid access token"));

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.InvalidRequest,
                    "Invalid user ID in token"));

            var storedRefreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
            if (storedRefreshToken == null || storedRefreshToken.UserId != userId)
                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.InvalidRequest,
                    "Invalid refresh token"));

            // Reuse detection: a revoked token being presented indicates the previous holder
            // already rotated past it — the real owner is likely an attacker. Revoke everything.
            if (storedRefreshToken.IsRevoked)
            {
                _logger.LogWarning(
                    "Refresh token reuse detected for user {UserId}. All sessions revoked. Source IP: {IpAddress}, UserAgent: {UserAgent}",
                    storedRefreshToken.UserId, ipAddress, userAgent);

                await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(
                    storedRefreshToken.UserId, ipAddress, userAgent, "Refresh token reuse detected");

                var compromisedUser = await _unitOfWork.Users.GetByIdAsync(storedRefreshToken.UserId);
                if (compromisedUser != null)
                {
                    compromisedUser.LastSessionsRevokedAt = DateTime.UtcNow;
                    _unitOfWork.Users.Update(compromisedUser);
                    await _unitOfWork.SaveChangesAsync();
                }

                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.InvalidRequest,
                    "Invalid refresh token"));
            }

            if (storedRefreshToken.IsExpired)
                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.InvalidRequest,
                    "Refresh token is not active"));

            var user = await _unitOfWork.Users.GetUserWithPermissionsAsync(userId);
            if (user == null)
                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.NotFound,
                    "User not found"));

            storedRefreshToken.Revoke(ipAddress, userAgent, "Token rotated");
            _unitOfWork.RefreshTokens.Update(storedRefreshToken);
            await _unitOfWork.SaveChangesAsync();

            var newAccessTokenResult = await GenerateAccessTokenAsync(user);
            if (!newAccessTokenResult.IsSuccess)
                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.InternalError,
                    "Failed to generate access token"));

            var newRefreshTokenResult = await GenerateRefreshTokenAsync(
                user,
                ipAddress,
                userAgent,
                storedRefreshToken.DeviceId,
                storedRefreshToken.DeviceName,
                rememberMe: false,
                preserveExpiryDate: storedRefreshToken.ExpiryDate);
            if (!newRefreshTokenResult.IsSuccess)
                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.InternalError,
                    "Failed to generate refresh token"));

            return Result<LoginResponseDto>.Success(
                BuildLoginResponse(user, newAccessTokenResult.Value, newRefreshTokenResult.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed for user");
            return Result<LoginResponseDto>.Failure(Error.Failure(
                ErrorCode.InternalError,
                "An unexpected error occurred during token refresh"));
        }
    }
    private static LoginResponseDto BuildLoginResponse(User user, string accessToken, RefreshTokenIssueResult refreshTokenResult)
    {
        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenResult.PlainToken,
            ExpiresAt = refreshTokenResult.Stored.ExpiryDate,
            User = new Application.Features.Users.Dtos.UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Status = user.Status,
                EmailConfirmed = user.EmailConfirmed,
                PhoneConfirmed = user.PhoneConfirmed,
                ProfileImageUrl = user.ProfileImageUrl,
                CreatedDate = user.CreatedDate,
                TenantId = user.TenantId,
                Roles = user.UserRoles.Select(ur => new Application.Features.Roles.Dtos.RoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description,
                    IsSystemRole = ur.Role.IsSystemRole,
                    CreatedDate = ur.Role.CreatedDate
                }).ToList(),
                Permissions = user.UserRoles
                    .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission))
                    .Distinct()
                    .Select(p => new Application.Features.Permissions.Dtos.PermissionDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        Resource = p.Resource,
                        Type = p.Type,
                        FullPermission = p.FullPermission
                    }).ToList()
            }
        };
    }

    public async Task<Result> RevokeTokenAsync(string refreshToken, string ipAddress = null, string userAgent = null, string reason = null)
        {
            try
            {
                await _unitOfWork.RefreshTokens.RevokeTokenAsync(refreshToken, ipAddress, userAgent, reason);
                await _unitOfWork.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke refresh token");
                return Result.Failure(Error.Failure(ErrorCode.InternalError, "Failed to revoke token"));
            }
        }

        public async Task<Result> RevokeAllUserTokensAsync(int userId, string ipAddress = null, string userAgent = null, string reason = null)
        {
            try
            {
                await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(userId, ipAddress, userAgent, reason);
                await _unitOfWork.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke all tokens for user {UserId}", userId);
                return Result.Failure(Error.Failure(ErrorCode.InternalError, "Failed to revoke tokens"));
            }
        }

        public async Task<Result> RevokeTokensByDeviceAsync(int userId, string deviceId, string ipAddress = null, string userAgent = null, string reason = null)
        {
            try
            {
                await _unitOfWork.RefreshTokens.RevokeTokensByDeviceAsync(userId, deviceId, ipAddress, userAgent, reason);
                await _unitOfWork.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to revoke tokens by device for user {UserId}", userId);
                return Result.Failure(Error.Failure(ErrorCode.InternalError, "Failed to revoke tokens"));
            }
        }

        public ClaimsPrincipal GetClaimsFromExpiredToken(string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false, // Don't validate lifetime for expired tokens
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 
