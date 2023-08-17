using ECharge.Domain.Enums;
using ECharge.Domain.EVtrip.DTOs.Requests;
using ECharge.Domain.EVtrip.Interfaces;
using ECharge.Domain.Job.Interface;
using ECharge.Infrastructure.Services.DatabaseContext;
using Microsoft.Extensions.DependencyInjection;

namespace ECharge.Infrastructure.Services.Quartz
{
    public class ChargeSession : IChargeSession
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ChargeSession(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Execute(string chargePointId)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var chargePointApiClient = scope.ServiceProvider.GetRequiredService<IChargePointApiClient>();
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var session = context.Sessions.OrderByDescending(x => x.Id).FirstOrDefault(x => x.ChargerPointId == chargePointId);

                if (session.Status == SessionStatus.NotCharging && !session.UpdatedTime.HasValue)
                {
                    await chargePointApiClient.StartChargingAsync(chargePointId, new StartChargingRequest { IgnoreDelay = true });
                    session.Status = SessionStatus.Charging;
                    session.UpdatedTime = DateTime.Now;

                }
                else if (session.Status == SessionStatus.Charging && session.UpdatedTime.HasValue)
                {
                    var chargingSession = await chargePointApiClient.GetChargingSessionsAsync(chargePointId);
                    await chargePointApiClient.StopChargingAsync(chargePointId, new StopChargingRequest { SessionId = chargingSession.SessionId });
                    session.Status = SessionStatus.Complated;
                    session.UpdatedTime = DateTime.Now;

                }

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

