using Kickstart.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kickstart.Domain.Common.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User,int>
    {
        Task<User> GetByEmailAsync(string email, int? tenantId);
        Task<IReadOnlyList<User>> GetUsersByEmailAsync(string email);
        Task<User> GetUserWithRolesAsync(int userId);
        Task<User> GetUserWithPermissionsAsync(int userId);
        Task<bool> EmailExistsAsync(string email, int? tenantId);
        /// <param name="excludeUserId">If set, that user is ignored (for updates).</param>
        Task<bool> PhoneExistsAsync(string phoneNumber, int? tenantId, int? excludeUserId = null);
        Task<UserRole> GetUserRoleAsync(int userId, int roleId);
        Task AddUserRoleAsync(UserRole userRole);
        void RemoveUserRole(UserRole userRole);
        Task<IEnumerable<User>> GetUsersWithRolesAsync(int page, int pageSize, string searchTerm = null, int? tenantId = null, bool excludeUsersWithSuperAdminRole = false);
        Task<int> GetUsersWithRolesCountAsync(string searchTerm = null, int? tenantId = null, bool excludeUsersWithSuperAdminRole = false);
    }
} 
