using {{PROJECT_NAME}}.Application.Interfaces;
using BCrypt.Net;
using {{PROJECT_NAME}}.Domain.Common.Interfaces;
using {{PROJECT_NAME}}.Domain.Common.Interfaces.Repositories;
using {{PROJECT_NAME}}.Domain.Entities;
using {{PROJECT_NAME}}.Application.Common.Results;
using System;
using System.Text;
using System.Text.RegularExpressions;
using {{PROJECT_NAME}}.Domain.Models;
using {{PROJECT_NAME}}.Domain.Common.Enums;

namespace {{PROJECT_NAME}}.Infrastructure.Services
{
    public class PasswordService : IPasswordService
    {
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
            return Result<string>.Failure(Error.Failure(
                ErrorCode.InternalError,
                $"Error hashing password: {ex.Message}"));
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
            return Result<bool>.Failure(Error.Failure(
                ErrorCode.InternalError,
                $"Error verifying password: {ex.Message}"));
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
                
                var random = new Random();
                var password = new StringBuilder();

                // Ensure at least one character from each category
                password.Append(lowercase[random.Next(lowercase.Length)]);
                password.Append(uppercase[random.Next(uppercase.Length)]);
                password.Append(digits[random.Next(digits.Length)]);
                password.Append(special[random.Next(special.Length)]);

                // Fill the rest randomly
                string allChars = lowercase + uppercase + digits + special;
                for (int i = 4; i < length; i++)
                {
                    password.Append(allChars[random.Next(allChars.Length)]);
                }

                // Shuffle the password
                var chars = password.ToString().ToCharArray();
                for (int i = chars.Length - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    (chars[i], chars[j]) = (chars[j], chars[i]);
                }

            return Result<string>.Success(new string(chars));
        }
            catch (Exception ex)
            {
            return Result<string>.Failure(
            Error.Failure(ErrorCode.ValidationFailed, $"Error generating password: {ex.Message}"));
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
