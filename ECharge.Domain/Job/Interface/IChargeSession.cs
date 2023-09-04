using ECharge.Domain.Enums;

namespace ECharge.Domain.Job.Interface
{
    public interface IChargeSession
    {
        Task<ChargeRequestStatus> Execute(string chargePointId, bool tryAgain);
    }
}