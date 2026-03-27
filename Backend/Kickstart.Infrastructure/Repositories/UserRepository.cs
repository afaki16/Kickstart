using Kickstart.Domain.Common.Interfaces;
using Kickstart.Domain.Common.Interfaces.Repositories;
using Kickstart.Domain.Constants;
using Kickstart.Domain.Entities;
using Kickstart.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kickstart.Infrastructure.Repositories;


public class UserRepository : RepositoryBase<User, int>, IUserRepository
{
    private readonly ApplicationDbContext _context;
    public UserRepository(ApplicationDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<User> GetByEmailAsync(string email, int? tenantId)
    {
        return await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == email && u.TenantId == tenantId);
    }

    public async Task<IReadOnlyList<User>> GetUsersByEmailAsync(string email)
    {
        return await _context.Set<User>().Where(u => u.Email == email).ToListAsync();
    }

    public async Task<User> GetUserWithRolesAsync(int userId)
    {
        return await _context.Set<User>()
            .Include(u => u.Tenant)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User> GetUserWithPermissionsAsync(int userId)
    {
        return await _context.Set<User>()
            .Include(u => u.Tenant)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<bool> EmailExistsAsync(string email, int? tenantId)
    {
        return await _context.Set<User>().AnyAsync(u => u.Email == email && u.TenantId == tenantId);
    }

    public async Task<bool> PhoneExistsAsync(string phoneNumber, int? tenantId, int? excludeUserId = null)
    {
        var query = _context.Set<User>().Where(u => u.PhoneNumber == phoneNumber && u.TenantId == tenantId);
        if (excludeUserId.HasValue)
            query = query.Where(u => u.Id != excludeUserId.Value);
        return await query.AnyAsync();
    }

    public async Task<UserRole> GetUserRoleAsync(int userId, int roleId)
    {
        return await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
    }

    public async Task AddUserRoleAsync(UserRole userRole)
    {
        await _context.UserRoles.AddAsync(userRole);
    }

    public void RemoveUserRole(UserRole userRole)
    {
        _context.UserRoles.Remove(userRole);
    }

    public async Task<IEnumerable<User>> GetUsersWithRolesAsync(int page, int pageSize, string searchTerm = null, int? tenantId = null, bool excludeUsersWithSuperAdminRole = false)
    {
        var query = _context.Set<User>().Include(u => u.UserRoles).ThenInclude(ur => ur.Role).AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(u => u.TenantId == tenantId.Value);
        }

        if (excludeUsersWithSuperAdminRole)
            query = query.Where(u => !u.UserRoles.Any(ur => ur.Role.Name == RoleNames.SuperAdmin));

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var searchTermLower = searchTerm.ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(searchTermLower) ||
                u.LastName.ToLower().Contains(searchTermLower) ||
                u.Email.ToLower().Contains(searchTermLower));
        }

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetUsersWithRolesCountAsync(string searchTerm = null, int? tenantId = null, bool excludeUsersWithSuperAdminRole = false)
    {
        var query = _context.Set<User>().AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(u => u.TenantId == tenantId.Value);
        }

        if (excludeUsersWithSuperAdminRole)
            query = query.Where(u => !u.UserRoles.Any(ur => ur.Role.Name == RoleNames.SuperAdmin));

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var searchTermLower = searchTerm.ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(searchTermLower) ||
                u.LastName.ToLower().Contains(searchTermLower) ||
                u.Email.ToLower().Contains(searchTermLower));
        }

        return await query.CountAsync();
    }
}
