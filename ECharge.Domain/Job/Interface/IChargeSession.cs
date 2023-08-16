namespace ECharge.Domain.Job.Interface
{
    public interface IChargeSession
    {
        Task Execute(string chargePointId);
    }
}