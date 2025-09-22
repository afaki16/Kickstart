using {{PROJECT_NAME}}.Domain.Common;

namespace {{PROJECT_NAME}}.Application.Services
{
    public interface IPasswordService
    {
        Result<string> HashPassword(string password);
        Result<bool> VerifyPassword(string password, string hashedPassword);
        Result<string> GenerateRandomPassword(int length = 12);
        Result<bool> ValidatePasswordStrength(string password);
    }
} 
