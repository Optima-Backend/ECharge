/*
using System;
using System.IO;
using System.Linq;
using ECharge.Domain.PulPal.Model;
using System.Text;
using System.Threading.Tasks;
using ECharge.Infrastructure.Services.PulPal.Utils;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ECharge.Infrastructure.Services.DatabaseContext;
using ECharge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ECharge.Api.Controllers
{
    public class PaymentController : Controller
    {
        private readonly DataContext _context;

        public PaymentController(DataContext context)
        {
            _context = context;
        }
        
        public class AcceptDeliveryCommand
        {
            public string ExternalId { get; set; }
            public string Message { get; set; }
            public bool Success { get; set; }
        }
        

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

            if (!string.IsNullOrEmpty(model?.ExternalId))
            {
                var transaction = await _context.Transactions.FirstOrDefaultAsync(x => x.ExternalId == model.ExternalId);

                if (localSignature == acceptedSignature)
                {

                    transaction.Message = "The bill has been paid successfully!";
                    transaction.PaymentDate = DateTime.Now;
                    transaction.Status = true;
                    transaction.StatusCode = 200;
                }
                else
                {
                    transaction.Message = "Something went wrong during payment process!";
                    transaction.PaymentDate = DateTime.Now;
                    transaction.StatusCode = 400;
                }

                _context.Transactions.Update(transaction);

                await _context.SaveChangesAsync();
            }
        }



        [Route("/api/echarge/generatelink")]
        [HttpPost]
        public async Task<IActionResult> GenerateLink(ChargepointTimeSpanModel timeSpanModel)
        {
            PulPalPayment payment = new();

            int price = 5;

            int minutesOfCharged = (timeSpanModel.StopDate - timeSpanModel.StartDate).Minutes;

            double hoursOfCharged = (double)minutesOfCharged / 60;
            

            var externalId = Guid.NewGuid().ToString();
            await _context.Transactions.AddAsync(new Transaction
            {
                CreatedDate = DateTime.Now,
                ExternalId = externalId,
                ChargerId = timeSpanModel.ChargepointId,
                StartDate = timeSpanModel.StartDate,
                StopDate = timeSpanModel.StopDate,
                Amount = hoursOfCharged * price

            });

            await _context.SaveChangesAsync();

            var model = new ExternalIdModel
            {
                Link = payment.GeneratePaymentUrl(1, "ECharge", "Payment for ECharge by customer", externalId),
                Message = "Payment URL has been created successfully. Please click the URL to pay the bill",
                Success = true
            };

            return Ok(model);

        }
    }
}
*/

