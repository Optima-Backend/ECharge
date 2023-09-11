using ECharge.Domain.Enums;
using ECharge.Domain.EVtrip.DTOs.Requests;
using ECharge.Domain.EVtrip.Interfaces;
using ECharge.Infrastructure.Services.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using ECharge.Domain.Entities;
using ECharge.Domain.Job.Interface;

namespace ECharge.Infrastructure.Services.Quartz
{
    public class ChargeSession : IChargeSession
    {
        private readonly IChargePointApiClient _chargePointApiClient;
        private readonly DataContext _context;

        public ChargeSession(IChargePointApiClient chargePointApiClient, DataContext context)
        {
            _chargePointApiClient = chargePointApiClient;
            _context = context;
        }

        public async Task<ChargeRequestStatus> Execute(string orderId, bool tryAgain)
        {
            var session = await _context.Sessions
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x => x.Order.OrderId == orderId);

            if (session == null)
                return ChargeRequestStatus.Error;

            if ((session.Status == SessionStatus.NotCharging && !session.UpdatedTime.HasValue) || tryAgain)
            {
                return await HandleStartCharging(session);
            }
            else if ((session.Status == SessionStatus.Charging && session.UpdatedTime.HasValue) || tryAgain)
            {
                return await HandleStopCharging(session);
            }
            else
            {
                return ChargeRequestStatus.Error;
            }

        }

        private async Task<ChargeRequestStatus> HandleStartCharging(Session session)
        {
            var providerSession = await _chargePointApiClient.StartChargingAsync(session.ChargerPointId, new StartChargingRequest { IgnoreDelay = true });

            if (providerSession != null && providerSession.Success)
            {
                session.Status = SessionStatus.Charging;
                session.StartDate = providerSession.Result.CreatedAt;
                session.UpdatedTime = DateTime.Now;
                session.ProviderSessionId = providerSession.Result.SessionId;
                session.ProviderStatus = ProviderChargingSessionStatus.active;
                session.EnergyConsumption = providerSession.Result.EnergyConsumption;

                await _context.SaveChangesAsync();
                return ChargeRequestStatus.StartSuccess;
            }

            await HandleChargeFailure(session);
            return ChargeRequestStatus.StartCanceled;
        }

        private async Task<ChargeRequestStatus> HandleStopCharging(Session session)
        {
            var chargingSession = await _chargePointApiClient.GetChargingSessionsAsync(session.ChargerPointId);

            if (chargingSession == null)
            {
                await HandleChargeFailure(session);
                return ChargeRequestStatus.StopCanceled;
            }

            var providerSession = await _chargePointApiClient.StopChargingAsync(session.ChargerPointId, new StopChargingRequest { SessionId = chargingSession.SessionId });

            if (providerSession != null && providerSession.Success)
            {
                session.Status = SessionStatus.Complated;
                session.EndDate = providerSession.Result.ClosedAt;
                session.UpdatedTime = DateTime.Now;
                session.ProviderStatus = ProviderChargingSessionStatus.closed;

                await _context.SaveChangesAsync();
                return ChargeRequestStatus.StopSuccess;
            }

            await HandleChargeFailure(session);
            return ChargeRequestStatus.StopCanceled;
        }

        private async Task HandleChargeFailure(Session session)
        {
            session.Status = SessionStatus.Canceled;
            session.StartDate = null;
            session.EndDate = null;
            session.UpdatedTime = DateTime.Now;
            session.ProviderStatus = ProviderChargingSessionStatus.cancled;

            await _context.SaveChangesAsync();
        }
    }
}
