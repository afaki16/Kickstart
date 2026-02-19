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
    public class TenantRepository : RepositoryBase<Tenant, int>, ITenantRepository
{
    private readonly ApplicationDbContext _context;

    public TenantRepository(ApplicationDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Tenant> GetByNameAsync(string name)
    {
        return await _context.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Name == name);
    }

    public async Task<Tenant> GetByDomainAsync(string domain)
    {
        return await _context.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Domain == domain);
    }

    public async Task<Tenant> GetTenantWithUsersAsync(int tenantId)
    {
        return await _context.Set<Tenant>()
            .Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.Id == tenantId);
    }

    public async Task<bool> NameExistsAsync(string name)
    {
        return await _context.Set<Tenant>()
            .AnyAsync(t => t.Name == name);
    }

    public async Task<bool> DomainExistsAsync(string domain)
    {
        return await _context.Set<Tenant>()
            .AnyAsync(t => t.Domain == domain);
    }
}
}