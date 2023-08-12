using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ECharge.Domain.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetByIdAsync<TKey>(TKey id);
        IQueryable<T> GetAllIQueryable();
        Task<List<T>> GetAllAsync();
        Task<List<T>> FindAsync(Expression<Func<T, bool>> expression, int skip = 0, int take = int.MaxValue, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null);
        Task<T> FindSingleAsync(Expression<Func<T, bool>> expression);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        Task SaveChangesAsync();
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}

