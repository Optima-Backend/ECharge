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
            if (command.PlannedStartDate >= command.PlannedEndDate)
            {

                return BadRequest(
                      new ErrorModel
                      {
                          StatusCode = 400,
                          Message = "Start date can't be bigger than end date"
                      });
            }

            if (command.PlannedStartDate <= DateTime.Now)
            {
                return BadRequest(new ErrorModel
                {
                    StatusCode = 400,
                    Message = "Start date exceeds. Make sure it is beyond than current date"
                });
            }

            if (command.PlannedEndDate <= DateTime.Now)
            {
                return BadRequest(
                  new ErrorModel
                  {
                      StatusCode = 400,
                      Message = "End date exceeds. Make sure it is beyond from current date"
                  });
            }

            if ((command.PlannedEndDate - command.PlannedStartDate).TotalMinutes < 2)
            {
                return BadRequest(new ErrorModel
                {
                    StatusCode = 400,
                    Message = "The Charging time span must be at least 30 minutes"
                });
            }

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

