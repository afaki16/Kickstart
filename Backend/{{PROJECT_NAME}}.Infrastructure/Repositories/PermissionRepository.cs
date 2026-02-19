using {{PROJECT_NAME}}.Domain.Common.Interfaces;
using {{PROJECT_NAME}}.Domain.Common.Interfaces.Repositories;
using {{PROJECT_NAME}}.Domain.Entities;
using {{PROJECT_NAME}}.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace {{PROJECT_NAME}}.Infrastructure.Repositories
{
    public class PermissionRepository : RepositoryBase<Permission, int>, IPermissionRepository
    {
    private readonly ApplicationDbContext _context;
    public PermissionRepository(ApplicationDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Permission> GetByNameAsync(string name)
    {
        return await _context.Set<Permission>().FirstOrDefaultAsync(p => p.Name == name);
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(int roleId)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission)
                .Distinct()
                .ToListAsync();
        }

    public async Task<bool> PermissionExistsAsync(string name)
    {
        return await _context.Set<Permission>().AnyAsync(p => p.Name == name);
    }
}
} 
