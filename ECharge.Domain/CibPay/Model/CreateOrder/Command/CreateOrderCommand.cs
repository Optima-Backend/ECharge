using System;
namespace ECharge.Domain.CibPay.Model.CreateOrder.Command
{
    public class CreateOrderCommand
    {
        public decimal Amount { get; set; }
        public string MerchantOrderId { get; set; }
        public string UserId { get; set; }
        public string ChargePointId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }
}

