using {{PROJECT_NAME}}.Domain.Common.Interfaces;
using {{PROJECT_NAME}}.Domain.Common.Interfaces.Repositories;
using {{PROJECT_NAME}}.Domain.Entities;
using {{PROJECT_NAME}}.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace {{PROJECT_NAME}}.Infrastructure.Repositories;


public class UserRepository : RepositoryBase<User, int>, IUserRepository
{
    private readonly ApplicationDbContext _context;
    public UserRepository(ApplicationDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        return await _context.Set<User>().FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> GetUserWithRolesAsync(int userId)
    {
        return await _context.Set<User>()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User> GetUserWithPermissionsAsync(int userId)
    {
        return await _context.Set<User>()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Set<User>().AnyAsync(u => u.Email == email);
    }

    public async Task<bool> PhoneExistsAsync(string phoneNumber)
    {
        return await _context.Set<User>().AnyAsync(u => u.PhoneNumber == phoneNumber);
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

    public async Task<IEnumerable<User>> GetUsersWithRolesAsync(int page, int pageSize, string searchTerm = null)
    {
        var query = _context.Set<User>().Include(u => u.UserRoles).ThenInclude(ur => ur.Role).AsQueryable();

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
}
