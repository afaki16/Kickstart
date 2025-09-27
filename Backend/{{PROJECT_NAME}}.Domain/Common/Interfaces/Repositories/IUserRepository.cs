using {{PROJECT_NAME}}.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace {{PROJECT_NAME}}.Application.Interfaces
{
    public interface IUserRepository : IRepository<User,int>
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> GetUserWithRolesAsync(int userId);
        Task<User> GetUserWithPermissionsAsync(int userId);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> PhoneExistsAsync(string phoneNumber);
        Task<UserRole> GetUserRoleAsync(int userId, int roleId);
        Task AddUserRoleAsync(UserRole userRole);
        void RemoveUserRole(UserRole userRole);
        Task<IEnumerable<User>> GetUsersWithRolesAsync(int page, int pageSize, string searchTerm = null);
    }
} 
