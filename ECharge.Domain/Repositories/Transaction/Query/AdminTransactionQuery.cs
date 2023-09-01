using ECharge.Domain.Enums;

namespace ECharge.Domain.Repositories.Transaction.Query
{
    public class TransactionQuery
    {
        public int PageSize { get; set; } = 10;
        public int PageIndex { get; set; } = 1;

        public string Id { get; set; }

        public string ChargerPointId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public double? DurationInMinutes { get; set; }

        public decimal? PricePerHour { get; set; }

        public SessionStatus? SessionStatus { get; set; }

        public DateTime? UpdatedTime { get; set; }

        public string UserId { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string ChargePointName { get; set; }

        public int? MaxVoltage { get; set; }

        public int? MaxAmperage { get; set; }

        public int? OrderId { get; set; }

        public string ProviderOrderId { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? Updated { get; set; }

        public string Currency { get; set; }

        public decimal? AmountCharged { get; set; }

        public PaymentStatus? OrderStatus { get; set; }

        public string MerchantOrderId { get; set; }

        public decimal? AmountRefunded { get; set; }

        public string Description { get; set; }

        public decimal? Amount { get; set; }

        public string Pan { get; set; }

        public DateTime? OrderCreatedAt { get; set; }

    }
}

