using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace {{PROJECT_NAME}}.Application.Interfaces;

public interface IRepository<TEntity, TKey> where TEntity : class
{
    IQueryable<TEntity> GetQueryable();
    Task<TEntity?> GetByIdAsync(TKey id);
    Task<TEntity> AddAsync(TEntity entity);
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expression);
    Task<TEntity> FindFirstAsync(Expression<Func<TEntity, bool>> expression);
    void UpdateRange(IEnumerable<TEntity> entities);
    void SoftDelete(TEntity entity);
    Task<IReadOnlyList<TEntity>> GetAllAsync();
    void Update(TEntity entity);
    Task UpdateAsync(TEntity entity);
    void Delete(TEntity entity);
    Task DeleteAsync(TEntity entity);
    Task<bool> ExistsAsync(TKey id);



}

