using ECharge.Domain.EVtrip.DTOs.Requests;
using ECharge.Domain.EVtrip.DTOs.Responses;
using ECharge.Domain.EVtrip.Models;

namespace ECharge.Domain.EVtrip.Interfaces
{
    public interface IChargePointApiClient
    {
        Task<List<ChargePointShortView>> GetAllChargePointsAsync();
        Task<List<ChargingSession>> GetChargingSessionsAsync(string chargepointId);
        Task<OperationResult<ChargingSession>> StartChargingAsync(string chargepointId, StartChargingRequest request);
        Task<OperationResult<ChargingSession>> StopChargingAsync(string chargepointId, StopChargingRequest request);
    }
}

