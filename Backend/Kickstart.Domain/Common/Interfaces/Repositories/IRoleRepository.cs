using Kickstart.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kickstart.Domain.Common.Interfaces.Repositories
{
    public interface IRoleRepository : IRepository<Role,int>
    {
        Task<Role> GetByNameAsync(string name);
        Task<Role> GetRoleWithPermissionsAsync(int roleId);
        Task<int> GetByIdWithNameAsync(string name);
        Task<IEnumerable<Role>> GetAllWithPermissionsAsync();
        Task<IEnumerable<Role>> GetRolesWithPermissionsPagedAsync(int page, int pageSize, string searchTerm = null, bool excludeSuperAdminRole = false);
        Task<int> GetRolesWithPermissionsCountAsync(string searchTerm = null, bool excludeSuperAdminRole = false);
        Task<IEnumerable<Role>> GetNonSystemRolesWithPermissionsAsync();
        Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId);
        Task<bool> RoleExistsAsync(string name);
        Task<bool> RoleHasPermissionAsync(int roleId, string permission);
        Task<IEnumerable<string>> GetRolePermissionsAsync(int roleId);
        Task<RolePermission> GetRolePermissionAsync(int roleId, int permissionId);
        Task AddRolePermissionAsync(RolePermission rolePermission);
        void RemoveRolePermission(RolePermission rolePermission);
    }
} 
