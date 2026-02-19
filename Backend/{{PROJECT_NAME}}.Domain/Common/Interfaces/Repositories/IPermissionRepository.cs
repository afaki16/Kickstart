using {{PROJECT_NAME}}.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace {{PROJECT_NAME}}.Domain.Common.Interfaces.Repositories
{
    public interface IPermissionRepository : IRepository<Permission,int>
    {
        Task<Permission> GetByNameAsync(string name);
        Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(int roleId);
        Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(int userId);
        Task<bool> PermissionExistsAsync(string name);
    }
} 
