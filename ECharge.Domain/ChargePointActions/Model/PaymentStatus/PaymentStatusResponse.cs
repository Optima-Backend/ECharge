using System.Net;
using ECharge.Domain.CibPay.Model;
using ECharge.Domain.Entities;

namespace ECharge.Domain.ChargePointActions.Model.PaymentStatus
{
    public class PaymentStatusResponse
    {
        public Session Session { get; set; }
        public SingleOrderResponse SingleOrderResponse { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
    }
}

