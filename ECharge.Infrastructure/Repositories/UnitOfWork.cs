//using ECharge.Domain.Repositories;

//namespace ECharge.Infrastructure.Repositories
//{
//    public class UnitOfWork : IUnitOfWork
//    {
//        private readonly Context _context;

//        private AnyRepository _anyRepository;

//        public AnyRepository AnyRepo => _anyRepository ??= new AnyRepository(_context);

//        public UnitOfWork(Context context)
//        {
//            _context = context;
//        }

//        public async Task<int> Complete()
//        {
//            return await _context.SaveChangesAsync();
//        }

//        public void Dispose() => _context.Dispose();
//    }
//}

