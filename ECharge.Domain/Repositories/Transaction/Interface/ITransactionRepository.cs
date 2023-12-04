using ECharge.Domain.CibPay.Model;
using ECharge.Domain.CibPay.Model.BaseResponse;
using ECharge.Domain.Repositories.Transaction.DTO;
using ECharge.Domain.Repositories.Transaction.Query;

namespace ECharge.Domain.Repositories.Transaction.Interface
{
    public interface ITransactionRepository
    {
        Task<BaseResponseWithPagination<AdminTransactionDTO>> GetAdminTransactions(TransactionQuery query);
        Task<BaseResponseWithPagination<NotificationDTO>> AllNotifications(NotificationQuery query);
        Task<BaseResponse<NotificationDTO>> SingleNotification(int id, string userId);
        Task<BaseResponseWithPagination<CableStateDTO>> GetCableStates(CableStateQuery query);
        Task<BaseResponseWithPagination<OrderStatusChangedDTO>> GetOrderStatusChangedHooks(OrderStatusChangedQuery query);
        Task<BaseResponse<TransactionReportDTO>> GetTransactionReport(GetOrdersQuery query);
        Task<BaseResponseWithPagination<SingleOrderResponse>> GetAllCibTransactions(GetOrdersQuery query);
        Task<ExcelFileResponse> GetTransactionExcel(GetOrdersQuery query);
        Task<BaseResponse<List<StatisticReport>>> GetStatisticsReport();
    }
}