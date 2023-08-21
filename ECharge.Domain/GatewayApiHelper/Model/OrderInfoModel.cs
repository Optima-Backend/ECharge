namespace ECharge.Domain.GatewayApiHelper.Model
{
    public class OrderInfoModel
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountCharged { get; set; }
        public decimal AmountRefunded { get; set; }
        public string AuthCode { get; set; }
        public CardModel Card { get; set; }
        public ClientModel Client { get; set; }
        public DateTime Created { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string Descriptor { get; set; }
        public IssuerModel Issuer { get; set; }
        public LocationModel Location { get; set; }
        public string MerchantOrderId { get; set; }
        public List<OperationModel> Operations { get; set; }
        public string Pan { get; set; }
        public Secure3DModel Secure3D { get; set; }
        public string Segment { get; set; }
        public string Status { get; set; }
        public DateTime? Updated { get; set; }
    }
}

