namespace ECharge.Domain.CibPay.Model
{
    public class GetOrdersQuery
    {
        public int? PageSize { get; set; } = 10;
        public int? PageIndex { get; set; } = 1;
        public string Status { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public string MerchantOrderId { get; set; }
        public string CardType { get; set; }
        public string CardSubtype { get; set; }
        public string IpAddress { get; set; }
        public string Expand { get; set; }
        public bool NeedArchive { get; set; }
    }
}

