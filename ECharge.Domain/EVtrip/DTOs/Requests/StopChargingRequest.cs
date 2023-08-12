using Newtonsoft.Json;

namespace ECharge.Domain.EVtrip.DTOs.Requests
{
    public class StopChargingRequest
    {
        [JsonProperty("sessionId")]
        public string? SessionId { get; set; }
    }
}

