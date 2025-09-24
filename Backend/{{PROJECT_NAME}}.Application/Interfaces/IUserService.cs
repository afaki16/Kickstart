using {{PROJECT_NAME}}.Application.Common.Results;
using {{PROJECT_NAME}}.Application.DTOs;
using {{PROJECT_NAME}}.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace {{PROJECT_NAME}}.Application.Services
{
    public interface IUserService
    {
        Task<Result<UserDto>> GetUserByIdAsync(int id);
        Task<Result<UserDto>> GetUserByEmailAsync(string email);
        Task<Result<IEnumerable<UserDto>>> GetAllUsersAsync(int page = 1, int pageSize = 10, string searchTerm = null);
        Task<Result<UserDto>> CreateUserAsync(CreateUserDto createUserDto);
        Task<Result<UserDto>> UpdateUserAsync(UpdateUserDto updateUserDto);
        Task<Result> DeleteUserAsync(int id);
        Task<Result> ChangeUserStatusAsync(int id, UserStatus status);
        Task<Result> ResetPasswordAsync(int id, string newPassword);
        Task<Result<bool>> EmailExistsAsync(string email);
        Task<Result<bool>> PhoneExistsAsync(string phoneNumber);
    }
} 