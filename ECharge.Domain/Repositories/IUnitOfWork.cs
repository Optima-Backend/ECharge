namespace ECharge.Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> Complete();
    }
}

