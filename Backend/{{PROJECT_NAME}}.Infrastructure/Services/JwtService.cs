using {{PROJECT_NAME}}.Application.Interfaces;
using {{PROJECT_NAME}}.Application.Features.Auth.Dtos;
using {{PROJECT_NAME}}.Domain.Common.Interfaces;
using {{PROJECT_NAME}}.Domain.Common.Interfaces.Repositories;
using {{PROJECT_NAME}}.Application.Common.Results;
using {{PROJECT_NAME}}.Domain.Entities;
using {{PROJECT_NAME}}.Domain.Models;
using {{PROJECT_NAME}}.Domain.Common.Enums;
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

namespace {{PROJECT_NAME}}.Infrastructure.Services
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

    public async Task<Result<LoginResponseDto>> LoginAsync(string email, string password, string ipAddress, string userAgent, string deviceId = null, string deviceName = null)
    {
        try
        {
            var user = await _unitOfWork.Users.GetUserWithPermissionsAsync(
                (await _unitOfWork.Users.FindFirstAsync(u => u.Email == email))?.Id ?? 0);

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

            var refreshTokenResult = await GenerateRefreshTokenAsync(user, ipAddress, userAgent);
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
                RefreshToken = refreshTokenResult.Value.Token,
                ExpiresAt = refreshTokenResult.Value.ExpiryDate,
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

                // Add role claims
                var roles = userWithPermissions.UserRoles.Select(ur => ur.Role).ToList();
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                }


                var jwtSettings = _configuration.GetSection("JwtSettings");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: jwtSettings["Issuer"],
                    audience: jwtSettings["Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiryInMinutes"])),
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

        public async Task<Result<RefreshToken>> GenerateRefreshTokenAsync(User user, string ipAddress, string userAgent, string deviceId = null, string deviceName = null)
        {
            try
            {
                // Revoke existing active tokens for the same device if deviceId is provided
                if (!string.IsNullOrEmpty(deviceId))
                {
                    await _unitOfWork.RefreshTokens.RevokeTokensByDeviceAsync(user.Id, deviceId, ipAddress, userAgent, "New login from same device");
                }

                var randomBytes = new byte[64];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomBytes);

                var refreshToken = new RefreshToken
                {
                    UserId = user.Id,
                    Token = Convert.ToBase64String(randomBytes),
                    ExpiryDate = DateTime.UtcNow.AddDays(int.Parse(_configuration["JwtSettings:RefreshTokenExpiryInDays"])),
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    DeviceId = deviceId,
                    DeviceName = deviceName,
                    DeviceType = GetDeviceType(userAgent),
                    Location = await GetLocationFromIpAsync(ipAddress)
                };

                await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
                await _unitOfWork.SaveChangesAsync();

               return Result<RefreshToken>.Success(refreshToken);
        }
            catch (Exception ex)
            {
              return Result<RefreshToken>.Failure(Error.Failure(ErrorCode.ValidationFailed, $"Error generating refresh token: {ex.Message}"));
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

            // Revoke old refresh token
            storedRefreshToken.IsRevoked = true;
            _unitOfWork.RefreshTokens.Update(storedRefreshToken);

            // Generate new tokens
            // Generate new tokens
            var newAccessTokenResult = await GenerateAccessTokenAsync(user);
            if (!newAccessTokenResult.IsSuccess)
                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.InternalError,
                    "Failed to generate access token"));

            var newRefreshTokenResult = await GenerateRefreshTokenAsync(user, ipAddress, userAgent);
            if (!newRefreshTokenResult.IsSuccess)
                return Result<LoginResponseDto>.Failure(Error.Failure(
                    ErrorCode.InternalError,
                    "Failed to generate refresh token"));

            var response = new LoginResponseDto
            {
                AccessToken = newAccessTokenResult.Value,
                RefreshToken = newRefreshTokenResult.Value.Token,
                ExpiresAt = newRefreshTokenResult.Value.ExpiryDate,
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
                    CreatedDate = user.CreatedDate
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
