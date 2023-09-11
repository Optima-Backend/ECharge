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
            if (command.Duration.TotalMinutes < 10)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "The Duration time must be at least 10 minutes"
                });
            }

            if (string.IsNullOrEmpty(command.FCMToken))
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "FCM Token can not be empty"
                });
            }

            if (command.Price <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Price amount should be more than 0"
                });
            }

            if (string.IsNullOrEmpty(command.ChargePointId))
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Charge point id should be entered"
                });
            }

            return await _chargePointAction.GenerateLink(command);
        }

        [Route("/api/echarge/payment-redirect-url")]
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
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

