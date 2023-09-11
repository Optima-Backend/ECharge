using System;
namespace ECharge.Domain.CibPay.Model.RefundOrder.Command
{
    public class RefundSpecificAmoundOrderCommand
    {
        public string OrderId { get; set; }
        public decimal RefundAmount { get; set; }
    }
}

