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
        public async Task<ActionResult<object>> GenerateLink(CreateSessionCommand command)
        {
            //if (command.Duration.TotalMinutes < 10)
            //{
            //    return BadRequest(new
            //    {
            //        StatusCode = 400,
            //        Message = "The Duration time must be at least 10 minutes"
            //    });
            //}

            return await _chargePointAction.GenerateLink(command);
        }

        [Route("/api/echarge/payment-redirect-url")]
        [HttpGet]
        //[ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> RedirectUrl(string order_id)
        {
            await _chargePointAction.PaymentHandler(order_id);

            var paymentStatus = await _chargePointAction.GetPaymentStatus(order_id);

            return View(paymentStatus);
        }

        [Route("/api/echarge/get-session-status")]
        [HttpGet]
        public async Task<object> GetSessionStatus(string orderId)
        {
            return await _chargePointAction.GetSessionStatus(orderId);
        }
    }
}

