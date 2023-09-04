namespace ECharge.Domain.EVtrip.Models
{
    public class CableStateChangedPayload
    {
        public string ChargerId { get; set; }
        public int Connector { get; set; }
        public string CableState { get; set; }
    }
}

