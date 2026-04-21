using Kickstart.Application.Interfaces;
using BCrypt.Net;
using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Domain.Entities;
using Kickstart.Application.Common.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Kickstart.Domain.Models;
using Kickstart.Domain.Common.Enums;

namespace Kickstart.Infrastructure.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(ILogger<PasswordService> logger)
        {
            _logger = logger;
        }

    public Result<string> HashPassword(string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(password))
                return Result<string>.Failure(Error.Failure(
                    ErrorCode.ValidationFailed,
                    "Password cannot be empty"));

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
            return Result<string>.Success(hashedPassword);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to hash password");
            return Result<string>.Failure(Error.Failure(
                ErrorCode.InternalError,
                "Failed to hash password"));
        }
    }

    public Result<bool> VerifyPassword(string password, string hashedPassword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
                return Result<bool>.Success(false);

            var isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            return Result<bool>.Success(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify password");
            return Result<bool>.Failure(Error.Failure(
                ErrorCode.InternalError,
                "Failed to verify password"));
        }
    }

    public Result<string> GenerateRandomPassword(int length = 12)
        {
            try
            {
                if (length < 8)
                return Result<string>.Failure(
            Error.Failure(ErrorCode.ValidationFailed, "Password must be at least 8 characters long"));

            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
                const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                const string digits = "0123456789";
                const string special = "!@#$%^&*";
                
                var password = new StringBuilder();

                password.Append(lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)]);
                password.Append(uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)]);
                password.Append(digits[RandomNumberGenerator.GetInt32(digits.Length)]);
                password.Append(special[RandomNumberGenerator.GetInt32(special.Length)]);

                string allChars = lowercase + uppercase + digits + special;
                for (int i = 4; i < length; i++)
                    password.Append(allChars[RandomNumberGenerator.GetInt32(allChars.Length)]);

                var chars = password.ToString().ToCharArray();
                for (int i = chars.Length - 1; i > 0; i--)
                {
                    int j = RandomNumberGenerator.GetInt32(i + 1);
                    (chars[i], chars[j]) = (chars[j], chars[i]);
                }

                return Result<string>.Success(new string(chars));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate random password");
                return Result<string>.Failure(
                    Error.Failure(ErrorCode.InternalError, "Failed to generate password"));
            }
        }

    public Result<bool> ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Result<bool>.Failure(
                Error.Failure(ErrorCode.ValidationFailed, "Password cannot be empty"));

        if (password.Length < 8)
            return Result<bool>.Failure(
                Error.Failure(ErrorCode.ValidationFailed, "Password must be at least 8 characters long"));

        if (!Regex.IsMatch(password, @"[a-z]"))
            return Result<bool>.Failure(
                Error.Failure(ErrorCode.ValidationFailed, "Password must contain at least one lowercase letter"));

        if (!Regex.IsMatch(password, @"[A-Z]"))
            return Result<bool>.Failure(
                Error.Failure(ErrorCode.ValidationFailed, "Password must contain at least one uppercase letter"));

        if (!Regex.IsMatch(password, @"\d"))
            return Result<bool>.Failure(
                Error.Failure(ErrorCode.ValidationFailed, "Password must contain at least one digit"));

        return Result<bool>.Success(true);
    }

}
} 
