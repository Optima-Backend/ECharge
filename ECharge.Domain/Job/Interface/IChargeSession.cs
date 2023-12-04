using ECharge.Domain.Enums;

namespace ECharge.Domain.Job.Interface
{
    public interface IChargeSession
    {
        Task<ChargeRequestStatus> Execute(string chargePointId, bool tryAgainForStart, bool tryAgainForStop,
            bool tryAgainForWebHookStatusB, bool isWebHookStatusB, bool tryAgainForStopByClient, bool isStoppedByClient);
    }
}