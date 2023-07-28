namespace ECharge.Domain.EVtrip.Models
{
    public class ChargePoint
    {
        public string Alias { get; set; }
        public string Id { get; set; }
        public string LastUpdatedAt { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int? MaxAmperage { get; set; }
        public int? MaxVoltage { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
    }
}

