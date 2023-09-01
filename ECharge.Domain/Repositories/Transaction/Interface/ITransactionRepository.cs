using ECharge.Domain.Repositories.Transaction.DTO;
using ECharge.Domain.Repositories.Transaction.Query;

namespace ECharge.Domain.Repositories.Transaction.Interface
{
    public interface ITransactionRepository
    {
        Task<BaseResponseWithPagination<AdminTransactionDTO>> GetAdminTransactions(TransactionQuery query);
    }
}