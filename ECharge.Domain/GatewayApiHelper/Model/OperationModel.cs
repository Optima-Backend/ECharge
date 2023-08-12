namespace ECharge.Domain.GatewayApiHelper.Model
{
    public class OperationModel
    {
        public string Type { get; set; }
        public string ISOMessage { get; set; }
        public DateTime Created { get; set; }
        public string Amount { get; set; }
        public string AuthCode { get; set; }
        public string ISOResponseCode { get; set; }
        public CashflowModel Cashflow { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string ARN { get; set; }
    }
}

