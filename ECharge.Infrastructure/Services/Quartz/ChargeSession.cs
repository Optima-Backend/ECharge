using ECharge.Domain.Entities;
using ECharge.Domain.Enums;
using ECharge.Domain.EVtrip.DTOs.Requests;
using ECharge.Domain.EVtrip.Interfaces;
using ECharge.Domain.Job.Interface;
using ECharge.Infrastructure.Services.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace ECharge.Infrastructure.Services.Quartz
{
    public class ChargeSession : IChargeSession
    {
        private readonly DataContext _context;
        private readonly IChargePointApiClient _chargePointApiClient;

        public ChargeSession(DataContext context, IChargePointApiClient chargePointApiClient)
        {
            _context = context;
            _chargePointApiClient = chargePointApiClient;
        }

        public async Task Execute(string chargePointId)
        {
            var session = await _context.Sessions.FirstOrDefaultAsync(x => x.ChargerPointId == chargePointId);

            if (session.Status == SessionStatus.NotCharging)
            {
                await StartChargingSession(chargePointId, session);
            }
            else if (session.Status == SessionStatus.Charging)
            {
                await StopChargingSession(chargePointId, session);
            }

        }

        private async Task StartChargingSession(string chargePointId, Session session)
        {
            await _chargePointApiClient.StartChargingAsync(chargePointId, new StartChargingRequest { IgnoreDelay = true });
            session.Status = SessionStatus.Charging;
            session.UpdatedTime = DateTime.Now;
            _context.Update(session);
            await _context.SaveChangesAsync();

        }

        private async Task StopChargingSession(string chargePointId, Session session)
        {
            var chargingSession = await _chargePointApiClient.GetChargingSessionsAsync(chargePointId);
            await _chargePointApiClient.StopChargingAsync(chargePointId, new StopChargingRequest { SessionId = chargingSession.SessionId });
            session.Status = SessionStatus.Complated;
            session.UpdatedTime = DateTime.Now;
            _context.Update(session);
            await _context.SaveChangesAsync();
        }
    }
}

