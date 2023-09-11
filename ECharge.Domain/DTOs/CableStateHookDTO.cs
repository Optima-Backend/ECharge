namespace ECharge.Domain.DTOs
{
    public class CableStateHookDTO
    {
        public string ChargePointId { get; set; }
        public string SessionId { get; set; }
        public int Connector { get; set; }
        public string CableState { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

