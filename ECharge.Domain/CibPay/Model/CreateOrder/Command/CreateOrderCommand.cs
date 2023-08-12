using System;
namespace ECharge.Domain.CibPay.Model.CreateOrder.Command
{
    public class CreateOrderCommand
    {
        public double Amount { get; set; }
        public string Currency { get; set; }
        public string MerchantOrderId { get; set; }
        public string InvoiceId { get; set; }
        public bool? AutoCharge { get; set; }
        public string ExpirationTimeout { get; set; }
        public int? Force3d { get; set; }
        public string Language { get; set; }
        public string ReturnUrl { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}

