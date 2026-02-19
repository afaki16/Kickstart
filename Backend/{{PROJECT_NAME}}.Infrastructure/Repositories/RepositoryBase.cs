using {{PROJECT_NAME}}.Domain.Common.Interfaces;
using {{PROJECT_NAME}}.Domain.Common.Interfaces.Repositories;
using {{PROJECT_NAME}}.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace {{PROJECT_NAME}}.Infrastructure.Repositories;

    public class RepositoryBase<T, TKey>(ApplicationDbContext context) : IRepository<T, TKey>
where T : class
{
    protected readonly ApplicationDbContext _context = context;
    public IQueryable<T> GetQueryable()
        => _context.Set<T>();

    public async Task<T?> GetByIdAsync(TKey id)
        => await _context.Set<T>().FindAsync(id);

    public async Task<IReadOnlyList<T>> GetAllAsync()
        => await _context.Set<T>().ToListAsync();

    public async Task<T> AddAsync(T entity)
    {
        await _context.Set<T>().AddAsync(entity);
        return entity;
    }

    public void Update(T entity)
        => _context.Set<T>().Update(entity);

    public async Task UpdateAsync(T entity)
    {
        _context.Set<T>().Update(entity);
        await Task.CompletedTask;
    }

    public void Delete(T entity)
        => _context.Set<T>().Remove(entity);

    public async Task DeleteAsync(T entity)
    {
        _context.Set<T>().Remove(entity);
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(TKey id)
    {
        return await _context.Set<T>().FindAsync(id) != null;
    }

    public void SoftDelete(T entity)
    {
        var property = entity.GetType().GetProperty("IsDeleted");
        if (property != null && property.PropertyType == typeof(bool))
        {
            property.SetValue(entity, true);
            Update(entity);
        }
    }

    public void UpdateRange(IEnumerable<T> entities)

         => _context.Set<T>().UpdateRange(entities);


    public void Remove(T entity)

        => _context.Set<T>().Remove(entity);


    public void RemoveRange(IEnumerable<T> entities)

         => _context.RemoveRange(entities);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
    {

        return await _context.Set<T>().Where(expression).ToListAsync();
    }

    public async Task<T> FindFirstAsync(Expression<Func<T, bool>> expression)
    {
        return await _context.Set<T>().FirstOrDefaultAsync(expression);
    }

}