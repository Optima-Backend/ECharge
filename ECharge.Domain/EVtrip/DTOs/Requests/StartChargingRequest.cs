namespace ECharge.Domain.EVtrip.DTOs.Requests
{
    public class StartChargingRequest
    {
        public bool IgnoreDelay { get; set; }
        public string SessionId { get; set; }
    }
}

