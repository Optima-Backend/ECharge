using System.ComponentModel.DataAnnotations;
using ECharge.Domain.Enums;

namespace ECharge.Domain.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public string OrderId { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public string Currency { get; set; }
        public decimal AmountCharged { get; set; }
        public PaymentStatus Status { get; set; }
        public string MerchantOrderId { get; set; }
        public decimal AmountRefunded { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Pan { get; set; }
    }
}

