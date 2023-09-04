namespace ECharge.Domain.EVtrip.Models
{
    public class ChargingSession
    {
        public string ChargerId { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public long? DurationInMinutes { get; set; }
        public double? EnergyConsumption { get; set; }
        public string SessionId { get; set; }
        public string Status { get; set; }
    }
}

