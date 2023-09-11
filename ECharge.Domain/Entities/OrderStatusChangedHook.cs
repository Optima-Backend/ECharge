using System.ComponentModel.DataAnnotations;

namespace ECharge.Domain.Entities
{
    public class OrderStatusChangedHook
    {
        [Key]
        public int Id { get; set; }
        public string ChargerId { get; set; }
        public int Connector { get; set; }
        public string OrderUuid { get; set; }
        public string Status { get; set; }
        public string FinishReason { get; set; }
        public DateTime CreatedDate { get; set; }
        public string SessionId { get; set; }
        public Session Session { get; set; }
    }
}

