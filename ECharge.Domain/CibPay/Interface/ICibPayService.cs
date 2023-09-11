using ECharge.Domain.CibPay.Model;
using ECharge.Domain.CibPay.Model.BaseResponse;
using ECharge.Domain.CibPay.Model.CreateOrder.Command;
using ECharge.Domain.CibPay.Model.CreateOrder.Response;
using ECharge.Domain.CibPay.Model.Ping.Response;
using ECharge.Domain.CibPay.Model.RefundOrder.Command;
using ECharge.Domain.CibPay.Model.RefundOrder.Response;

namespace ECharge.Domain.CibPay.Interface
{
    public interface ICibPayService
    {
        Task<CibBaseResponse<PingResponse>> GetPingResponse();
        Task<CibBaseResponse<AllOrdersResponse>> GetOrderInfo(string orderId);
        Task<CibBaseResponse<AllOrdersResponse>> GetOrdersList(GetOrdersQuery query);
        Task<CibBaseResponse<CreateOrderProviderResponse>> CreateOrder(CreateOrderCommand command);
        Task<CibBaseResponse<RefundOrderResponse>> RefundOrder(RefundOrderCommand command);
        Task<CibBaseResponse<RefundOrderResponse>> RefundSpecificAmount(RefundSpecificAmoundOrderCommand command);
    }
}

