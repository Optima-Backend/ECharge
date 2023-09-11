using ECharge.Domain.DTOs;
using ECharge.Domain.Enums;
using System.Net;

namespace ECharge.Domain.ChargePointActions.Model.PaymentStatus
{
    public class SessionStatusResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public int ChargingStatusCode { get; set; }
        public string Message { get; set; }
        public int RemainingStartTimeInSeconds { get; set; }
        public int RemainingChargingTimeInSeconds { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int DurationInSeconds { get; set; }
        public string ChargingStatusDescription { get; set; }
        public string ChargePointId { get; set; }
        public decimal PricePerHour { get; set; }
        public int? MaxAmperage { get; set; }
        public int? MaxVoltage { get; set; }
        public string Name { get; set; }
        public CableState? CableStatus { get; set; }
        public CableStateHookDTO LastCableState { get; set; }

    }
}

