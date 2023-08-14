using ECharge.Domain.GatewayApiHelper.Model;

namespace ECharge.Domain.GatewayApiHelper.Interface
{
    public interface IGatewayApiService
    {
        Task<PingResponse> GetPingResponse();
        Task<OrderInfoModel> GetOrderInfo(string orderId);
        Task<OrderListModel> GetOrdersList(string status = null, DateTime? createdFrom = null, DateTime? createdTo = null,
            string merchantOrderId = null, string cardType = null, string cardSubtype = null, string ipAddress = null,
            string expand = null);
        Task<OperationListModel> GetOperationsList(string expand = null, string status = null, string type = null,
            DateTime? createdFrom = null, DateTime? createdTo = null);
        Task<ExchangeRateModel> GetExchangeRates(DateTime? date = null, DateTime? dateFrom = null, DateTime? dateTo = null);
        Task<OrderCreateResponseModel> CreateOrder(decimal amount, string currency = "USD", string merchantOrderId = null,
            string segment = null);
        Task<OrderAuthorizeResponseModel> AuthorizeOrder(decimal amount, string pan, CardModel card,
            LocationInfoModel location = null, string currency = "USD", string merchantOrderId = null,
            string description = null, string clientAddress = null, string clientCity = null, string clientCountry = null,
            string clientEmail = null, string clientName = null, string clientPhone = null, string clientState = null,
            string clientZip = null, int? force3d = null, string returnUrl = null, int? autoCharge = null,
            string terminal = null, int? recurring = null, string secure3d20ReturnUrl = null, int? exemptionMit = null);
        Task<OrderReverseResponseModel> ReverseOrder(string orderId);
        Task<OrderChargeResponseModel> ChargeOrder(string orderId, decimal chargeAmount);
        Task<OrderRefundResponseModel> RefundOrder(string orderId, decimal refundAmount);
        Task<OrderCancelResponseModel> CancelOrder(string orderId, decimal? refundAmount = null);
        Task<RebillResponseModel> PerformRebill(string orderId, decimal amount, string cvv, string clientIp, bool recurring = false);
        Task<CreditResponseModel> PerformCredit(string orderId, decimal amount, string currency, string clientIp);
        Task<CreditResponseModel> PerformOriginalCredit(decimal amount, string pan);
        Task<CreditResponseModel> PerformOriginalCreditWithoutLink(decimal amount, string pan);
        Task<CompleteResponseModel> Complete3DSecureAuthentication(string orderId, string paRes);
        Task<Complete3D20ResponseModel> Complete3D20Authentication(string orderId);
        Task<Resume3DSResponseModel> Resume3DSAuthentication(string orderId);
    }
}

