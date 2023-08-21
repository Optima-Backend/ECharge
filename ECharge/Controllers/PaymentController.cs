using Microsoft.AspNetCore.Mvc;
using ECharge.Domain.ChargePointActions.Interface;
using ECharge.Domain.ChargePointActions.Model.CreateSession;
using ECharge.Domain.ErrorModels;

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
            var startTime = new DateTime(command.PlannedStartDate.Year, command.PlannedStartDate.Month, command.PlannedStartDate.Day, command.PlannedStartDate.Hour, command.PlannedStartDate.Minute, 0);
            var endTime = new DateTime(command.PlannedEndDate.Year, command.PlannedEndDate.Month, command.PlannedEndDate.Day, command.PlannedEndDate.Hour, command.PlannedEndDate.Minute, 0);

            if (command.PlannedStartDate >= command.PlannedEndDate)
            {

                return BadRequest(
                      new ErrorModel
                      {
                          StatusCode = 400,
                          Message = "Start time should be less than end time"
                      });
            }

            if (command.PlannedStartDate < DateTime.Now)
            {
                return BadRequest(new ErrorModel
                {
                    StatusCode = 400,
                    Message = "Start time exceeds. Make sure it is beyond than current time"
                });
            }

            if (command.PlannedEndDate <= DateTime.Now)
            {
                return BadRequest(
                  new ErrorModel
                  {
                      StatusCode = 400,
                      Message = "End time exceeds. Make sure it is beyond from current time"
                  });
            }

            if ((command.PlannedEndDate - command.PlannedStartDate).TotalMinutes < 10)
            {
                return BadRequest(new ErrorModel
                {
                    StatusCode = 400,
                    Message = "The Charging time span must be at least 10 minutes"
                });
            }

            return await _chargePointAction.GenerateLink(command);
        }

        [Route("/api/echarge/payment-redirect-url")]
        [HttpGet]
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

