namespace ECharge.Domain.CibPay.Model.RefundOrder.Command
{
    public class RefundOrderCommand
    {
        public string OrderId { get; set; }
        public decimal RefundAmount { get; set; }
    }
}

