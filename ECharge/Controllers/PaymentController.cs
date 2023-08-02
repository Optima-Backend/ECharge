using ECharge.Domain.PulPal.Model;
using System.Text;
using ECharge.Infrastructure.Services.PulPal.Utils;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ECharge.Api.Controllers
{
    public class PaymentController : Controller
    {
        [Route("/api/echarge/acceptdelivery")]
        [HttpPost]
        public async Task AcceptPayment()
        {
            var acceptDeliveryCommand = new AcceptDeliveryCommand();
            var nonce = Request.Headers["Nonce"].FirstOrDefault();
            var acceptedSignature = Request.Headers["Signature"].FirstOrDefault();

            if (string.IsNullOrEmpty(nonce) || string.IsNullOrEmpty(acceptedSignature))
            {
                acceptDeliveryCommand.Message = "One or more header values are not correct";
            }

            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var json = await reader.ReadToEndAsync();
            var model = JsonConvert.DeserializeObject<PulPalPaymentDeliveryModel>(json);
            var localSignature = PulPalPayment.GetDeliverySignature(UriHelper.GetDisplayUrl(Request), nonce, json);

            if (localSignature != acceptedSignature)
            {
                acceptDeliveryCommand.Message = "Invalid signature";
            }
            else
            {
                acceptDeliveryCommand.Success = true;
                acceptDeliveryCommand.Message = "Payment success!";
                acceptDeliveryCommand.ExternalId = model.ExternalId;
            }

        }

        public class AcceptDeliveryCommand
        {
            public string ExternalId { get; set; }
            public string Message { get; set; }
            public bool Success { get; set; }
        }

        [Route("/api/echarge/generatelink")]
        [HttpGet]
        public IActionResult GenerateLink()
        {
            PulPalPayment payment = new();

            var a = "123";
            var externalId = Guid.NewGuid().ToString();

            return Ok(payment.GeneratePaymentUrl(1, "ECharge", "Payment for ECharge by customer", Guid.NewGuid().ToString()));
        }
    }
}

