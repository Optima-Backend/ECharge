using ECharge.Domain.ChargePointActions.Model.CreateSession;
using ECharge.Domain.ChargePointActions.Model.PaymentStatus;

namespace ECharge.Domain.ChargePointActions.Interface
{
    public interface IChargePointAction
    {
        Task<object> GenerateLink(CreateSessionCommand command);
        Task PaymentHandler(string orderId);
        Task<PaymentStatusResponse> GetPaymentStatus(string orderId);
        Task<SessionStatusResponse> GetSessionStatus(string orderId);
        Task StopByClient(string orderId);
    }
}

