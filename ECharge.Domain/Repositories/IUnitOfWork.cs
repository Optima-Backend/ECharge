using System;
using System.Threading.Tasks;

namespace ECharge.Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> Complete();
    }
}

