using System.ComponentModel.DataAnnotations;

namespace ECharge.Domain.Entities
{
    public class CableStateHook
    {
        [Key]
        public int Id { get; set; }
        public string ChargePointId { get; set; }
        public string? SessionId { get; set; }
        public int Connector { get; set; }
        public string CableState { get; set; }
        public DateTime CreatedDate { get; set; }
        public Session Session { get; set; }
    }
}

