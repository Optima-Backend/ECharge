namespace ECharge.Domain.EVtrip.Models
{
    public class OrderStatusChangedPayload
    {
        public string ChargerId { get; set; }
        public int Connector { get; set; }
        public string OrderUuid { get; set; }
        public string Status { get; set; }
        public string FinishReason { get; set; }
    }
}

