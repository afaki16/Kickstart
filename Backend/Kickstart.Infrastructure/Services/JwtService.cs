using Kickstart.Application.Interfaces;
using Kickstart.Application.Features.Auth.Dtos;
using Kickstart.Application.Features.Auth.Models;
using Kickstart.Infrastructure.Security;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Application.Common.Results;
using Kickstart.Domain.Entities;
using Kickstart.Domain.Models;
using Kickstart.Domain.Common.Enums;
using Microsoft.Extensions.Configuration;
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

        public JwtService(IConfiguration configuration, IUnitOfWork unitOfWork, IPasswordService passwordService)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
        }

    public async Task<Result<LoginResponseDto>> LoginAsync(string email, string password, string ipAddress, string userAgent, string deviceId = null, string deviceName = null, bool rememberMe = false, int? tenantId = null)
    {
        try
        {
            var candidates = await _unitOfWork.Users.GetUsersByEmailAsync(email);
            if (candidates.Count == 0)
                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.InvalidRequest,
                    "Invalid email or password"));

            User resolved;
            if (candidates.Count == 1)
            {
                resolved = candidates[0];
            }
            else
            {
                if (!tenantId.HasValue)
                    return Result<LoginResponseDto>.Failure(Error.Failure(
                        ErrorCode.InvalidRequest,
                        "Multiple accounts with this email. Specify tenant id."));

                resolved = candidates.FirstOrDefault(u => u.TenantId == tenantId);
                if (resolved == null)
                    return Result<LoginResponseDto>.Failure(Error.Failure(
                        ErrorCode.InvalidRequest,
                        "Invalid email or password"));
            }

            var user = await _unitOfWork.Users.GetUserWithPermissionsAsync(resolved.Id);

            if (user == null)
                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.InvalidRequest,
                    "Invalid email or password"));

            var passwordVerification = _passwordService.VerifyPassword(password, user.PasswordHash);
            if (!passwordVerification.IsSuccess || !passwordVerification.Value)
                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.InvalidRequest,
                    "Invalid email or password"));

            if (user.Status != Domain.Common.Enums.UserStatus.Active)
                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.InvalidRequest,
                    "Account is not active"));

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

            user.LastLoginDate = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            var response = new LoginResponseDto
            {
                AccessToken = accessTokenResult.Value,
                RefreshToken = refreshTokenResult.Value.PlainToken,
                ExpiresAt = refreshTokenResult.Value.Stored.ExpiryDate,
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

            return Result<LoginResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<LoginResponseDto>.Failure(Error.Failure(
                ErrorCode.InternalError,
                $"Login failed: {ex.Message}"));
        }
    }
    public async Task<Result<string>> GenerateAccessTokenAsync(User user)
        {
            try
            {
                var userWithPermissions = await _unitOfWork.Users.GetUserWithPermissionsAsync(user.Id);
                
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
                var roles = userWithPermissions.UserRoles.Select(ur => ur.Role).ToList();
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
               return Result<string>.Failure(Error.Failure(ErrorCode.ValidationFailed, $"Error generating access token: {ex.Message}"));
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
              return Result<RefreshTokenIssueResult>.Failure(Error.Failure(ErrorCode.ValidationFailed, $"Error generating refresh token: {ex.Message}"));
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

            if (!storedRefreshToken.IsActive)
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

            var response = new LoginResponseDto
            {
                AccessToken = newAccessTokenResult.Value,
                RefreshToken = newRefreshTokenResult.Value.PlainToken,
                ExpiresAt = newRefreshTokenResult.Value.Stored.ExpiryDate,
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

            return Result<LoginResponseDto>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<LoginResponseDto>.Failure(Error.Failure(
                ErrorCode.InternalError,
                $"Error refreshing token: {ex.Message}"));
        }
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
            return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, $"Error revoking user tokens: {ex.Message}"));
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
            return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, $"Error revoking user tokens: {ex.Message}"));
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
            return Result.Failure(Error.Failure(ErrorCode.ValidationFailed, $"Error revoking user tokens: {ex.Message}"));
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
