namespace ECharge.Domain.CibPay.Model.CreateOrder.Request
{
    public class CreateOrderRequest
    {
        public double Amount { get; set; }
        public string Currency { get; set; }
        public ExtraFields ExtraFields { get; set; }
        public string MerchantOrderId { get; set; }
        public Options Options { get; set; }
        public Client Client { get; set; }
    }

    public class ExtraFields
    {
        public string InvoiceId { get; set; }
    }

    public class Options
    {
        public bool AutoCharge { get; set; }
        public string ExpirationTimeout { get; set; }
        public int Force3d { get; set; }
        public string Language { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class Client
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}

