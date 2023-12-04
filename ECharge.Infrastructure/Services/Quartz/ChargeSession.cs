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

        public async Task<ChargeRequestStatus> Execute(string orderId, bool tryAgainForStart, bool tryAgainForStop,
            bool tryAgainForWebHookStatusB, bool isWebHookStatusB, bool tryAgainForStopByClient,
            bool isStoppedByClient)
        {
            var session = await _context.Sessions
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x => x.Order.OrderId == orderId);

            if (session == null)
                return ChargeRequestStatus.Error;

            if ((session.Status == SessionStatus.NotCharging && !session.UpdatedTime.HasValue && !isWebHookStatusB &&
                 !isStoppedByClient) || tryAgainForStart) 
            {
                return await HandleStartCharging(session);
            }

            if ((session.Status == SessionStatus.Charging && session.UpdatedTime.HasValue) || tryAgainForStop ||
                tryAgainForWebHookStatusB || isWebHookStatusB || tryAgainForStopByClient || isStoppedByClient) 
            {
                return await HandleStopCharging(session, isWebHookStatusB, isStoppedByClient);
            }
            
            return ChargeRequestStatus.Error;
        }

        private async Task<ChargeRequestStatus> HandleStartCharging(Session session)
        {
            var providerSession = await _chargePointApiClient.StartChargingAsync(session.ChargerPointId,
                new StartChargingRequest { IgnoreDelay = true });

            if (providerSession is {Result: not null, Success: true })
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

        private async Task<ChargeRequestStatus> HandleStopCharging(Session session, bool isWebHookStatusB, bool isStoppedByClient)
        {
            var chargingSession = await _chargePointApiClient.GetChargingSessionsAsync(session.ChargerPointId);

            if (chargingSession == null)
            {
                await HandleChargeFailure(session);
                return ChargeRequestStatus.StopCanceled;
            }

            var providerSession = await _chargePointApiClient.StopChargingAsync(session.ChargerPointId,
                new StopChargingRequest { SessionId = chargingSession.SessionId });

            if (providerSession is { Success: true })
            {
                if (isWebHookStatusB)
                {
                    session.Status = SessionStatus.WebhookCanceled;
                    session.ProviderStatus = ProviderChargingSessionStatus.closed;
                }
                else if (isStoppedByClient)
                {
                    session.StoppedByClient = true;
                }
                else
                {
                    session.Status = SessionStatus.Complated;
                    session.ProviderStatus = ProviderChargingSessionStatus.closed;
                }

                session.EndDate = providerSession.Result.ClosedAt;
                session.UpdatedTime = DateTime.Now;

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
