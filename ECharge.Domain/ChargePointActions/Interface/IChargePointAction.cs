using ECharge.Domain.ChargePointActions.Model.CreateSession;

namespace ECharge.Domain.ChargePointActions.Interface
{
    public interface IChargePointAction
    {
        Task<object> GenerateLink(CreateSessionCommand command);
        Task PaymentHandler(string orderId);
        Task<object> GetPaymentStatus(string orderId);
    }
}

