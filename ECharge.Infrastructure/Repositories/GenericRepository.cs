//using System.Linq.Expressions;
//using ECharge.Domain.Repositories;

//namespace ECharge.Infrastructure.Repositories
//{
//    public class GenericRepository<T> : IGenericRepository<T> where T : class
//    {
//        protected readonly Context _context;

//        public GenericRepository(Context context)
//        {
//            _context = context;
//        }

//        public async Task AddAsync(T entity)
//        {
//            await _context.Set<T>().AddAsync(entity);
//        }

//        public async Task AddRangeAsync(IEnumerable<T> entities)
//        {
//            await _context.Set<T>().AddRangeAsync(entities);
//        }

//        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> expression, int skip = 0, int take = int.MaxValue, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null)
//        {
//            IQueryable<T> query = _context.Set<T>().Where(expression);

//            if (orderBy != null)
//            {
//                query = orderBy(query);
//            }

//            query = query.Skip(skip).Take(take);

//            return await query.ToListAsync();
//        }

//        public async Task<T> FindSingleAsync(Expression<Func<T, bool>> expression)
//        {
//            return await _context.Set<T>().FirstOrDefaultAsync(expression);
//        }

//        public async Task<List<T>> GetAllAsync()
//        {
//            return await _context.Set<T>().ToListAsync();
//        }

//        public IQueryable<T> GetAllIQueryable()
//        {
//            return _context.Set<T>();
//        }

//        public async Task<T> GetByIdAsync<TKey>(TKey id)
//        {
//            return await _context.Set<T>().FindAsync(id);
//        }

//        public void Remove(T entity)
//        {
//            _context.Set<T>().Remove(entity);
//        }

//        public void RemoveRange(IEnumerable<T> entities)
//        {
//            _context.Set<T>().RemoveRange(entities);
//        }

//        public void Update(T entity)
//        {
//            _context.Set<T>().Attach(entity);
//            _context.Entry(entity).State = EntityState.Modified;
//        }

//        public async Task SaveChangesAsync()
//        {
//            await _context.SaveChangesAsync();
//        }

//    }
//}

