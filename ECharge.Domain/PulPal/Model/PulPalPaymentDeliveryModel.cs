namespace ECharge.Domain.PulPal.Model
{
    public class PulPalPaymentDeliveryModel
    {
        public int Price { get; set; }
        public int ProductType { get; set; }
        public string ExternalId { get; set; }
        public bool Repeatable { get; set; }
        public int Debt { get; set; }
        public int Amount { get; set; }
        public int ProviderType { get; set; }
        public string PaymentAttempt { get; set; }
    }
}

