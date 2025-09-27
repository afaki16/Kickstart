using {{PROJECT_NAME}}.Application.Interfaces;
using {{PROJECT_NAME}}.Domain.Entities;
using {{PROJECT_NAME}}.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace {{PROJECT_NAME}}.Infrastructure.Repositories;

public class RoleRepository : RepositoryBase<Role, int>, IRoleRepository
{

    private readonly ApplicationDbContext _context;
    public RoleRepository(ApplicationDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Role> GetByNameAsync(string name)
    {
        return await _context.Set<Role>().FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<Role> GetRoleWithPermissionsAsync(int roleId)
    {
        return await _context.Set<Role>()
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId);
    }

    public async Task<IEnumerable<Role>> GetAllWithPermissionsAsync()
    {
        return await _context.Set<Role>()
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => ur.Role)
            .ToListAsync();
    }

    public async Task<bool> RoleExistsAsync(string name)
    {
        return await _context.Set<Role>().AnyAsync(r => r.Name == name);
    }

    public async Task<bool> RoleHasPermissionAsync(int roleId, string permission)
    {
        return await _context.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .AnyAsync(rp => rp.RoleId == roleId && rp.Permission.Name == permission);
    }

    public async Task<IEnumerable<string>> GetRolePermissionsAsync(int roleId)
    {
        return await _context.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission.Name)
            .ToListAsync();
    }

    public async Task<RolePermission> GetRolePermissionAsync(int roleId, int permissionId)
    {
        return await _context.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
    }

    public async Task AddRolePermissionAsync(RolePermission rolePermission)
    {
        await _context.RolePermissions.AddAsync(rolePermission);
    }

    public void RemoveRolePermission(RolePermission rolePermission)
    {
        _context.RolePermissions.Remove(rolePermission);
    }
}
