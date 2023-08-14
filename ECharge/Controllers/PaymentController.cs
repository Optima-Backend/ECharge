using Microsoft.AspNetCore.Mvc;
using ECharge.Domain.ChargePointActions.Interface;
using ECharge.Domain.ChargePointActions.Model.CreateSession;

namespace ECharge.Api.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IChargePointAction _chargePointAction;

        public PaymentController(IChargePointAction chargePointAction)
        {
            _chargePointAction = chargePointAction;
        }

        [Route("/api/echarge/generatelink")]
        [HttpPost]
        public async Task<object> GenerateLink(CreateSessionCommand command)
        {
            return await _chargePointAction.GenerateLink(command);
        }

        [Route("/api/echarge/payment-redirect-url")]
        [HttpGet]
        public async Task RedirectUrl(string order_id)
        {
            await _chargePointAction.PaymentHandler(order_id);
        }

        [Route("/api/echarge/get-payment-status")]
        [HttpGet]
        public async Task<object> GetPaymentStatus(string orderId)
        {
            return await _chargePointAction.GetPaymentStatus(orderId);
        }
    }
}

