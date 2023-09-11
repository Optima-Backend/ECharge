using ECharge.Domain.ChargePointActions.Interface;
using ECharge.Domain.CibPay.Interface;
using ECharge.Domain.CibPay.Model.RefundOrder.Command;
using ECharge.Domain.Entities;
using ECharge.Domain.Enums;
using ECharge.Domain.EVtrip.Interfaces;
using ECharge.Domain.EVtrip.Models;
using ECharge.Infrastructure.Services.DatabaseContext;
using ECharge.Infrastructure.Services.FirebaseNotification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECharge.Api.Controllers
{
    [Route("charger")]
    public class WebhookController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly ICibPayService _cibPayService;
        private readonly IChargePointApiClient _chargePointApiClient;
        private readonly IChargePointAction _chargePointAction;

        public WebhookController(DataContext dataContext, ICibPayService cibPayService, IChargePointApiClient chargePointApiClient, IChargePointAction chargePointAction)
        {
            _dataContext = dataContext;
            _cibPayService = cibPayService;
            _chargePointApiClient = chargePointApiClient;
            _chargePointAction = chargePointAction;
        }

        [HttpPost("cable-state-changed")]
        public async Task HandleCableStateChanged([FromBody] CableStateChangedPayload payload)
        {
            if (payload != null)
            {
                var session = _dataContext.Sessions
                    .AsNoTracking()
                    .Include(x => x.Order)
                    .Where(x => x.ChargerPointId == payload.ChargerId && x.Status == SessionStatus.Charging)
                    .FirstOrDefault();

                if (session == null) return;

                var orderId = session.Order.OrderId;

                await _dataContext.CableStateHooks.AddAsync(new CableStateHook
                {
                    CableState = payload.CableState,
                    ChargePointId = payload.ChargerId,
                    Connector = payload.Connector,
                    CreatedDate = DateTime.Now,
                    SessionId = session.Id
                });

                var updatedOrder = await _dataContext.Orders.FindAsync(session.OrderId);

                string title = string.Empty;
                string body = string.Empty;

                if (payload.CableState == "A")
                {
                    updatedOrder.CableState = CableState.A;
                    title = "Kabel əlaqəsi kəsildi";
                    body = "30 saniyə ərzində kabel ələqəsini təmin etməsəniz sessiyanız bağlanacaq və qalıq balans kartınıza geri göndəriləcək";

                }
                else if (payload.CableState == "B")
                {
                    updatedOrder.CableState = CableState.B;
                }
                else if (payload.CableState == "C")
                {
                    updatedOrder.CableState = CableState.C;
                    title = "Kabel əlaqəsi bərpa edildi";
                    body = "Kabel əlaqəsi yenidən təmin edildiyi üçün sessiya davam edir";
                }
                else if (payload.CableState == "D")
                {
                    updatedOrder.CableState = CableState.D;
                }
                else if (payload.CableState == "E")
                {
                    updatedOrder.CableState = CableState.E;
                }
                else if (payload.CableState == "F")
                {
                    updatedOrder.CableState = CableState.F;
                }

                await _dataContext.SaveChangesAsync();

                Notification.PushNotification(new FirebasePayload { FCMToken = session.FCMToken, Title = title, Body = body });

                #region SignalR
                //var connectionId = ChargerHub.GetConnectionIdByOrderId(orderId);
                //if (!string.IsNullOrEmpty(connectionId))
                //{
                //    var sessionStatusResponse = await _chargePointAction.GetSessionStatus(orderId);
                //    var hubUser = _hubContext.Clients.Clients(connectionId);
                //    await hubUser.SendAsync("From_Server_To_Client", sessionStatusResponse);
                //}
                #endregion
            }
        }

        [HttpPost("order/status-changed")]
        public async Task HandleOrderStatusChanged([FromBody] OrderStatusChangedPayload payload)
        {
            if (payload is not null)
            {
                var session = _dataContext.Sessions
                   .Include(x => x.Order)
                   .Where(x => x.ChargerPointId == payload.ChargerId && x.Status == SessionStatus.Charging && x.ProviderStatus == ProviderChargingSessionStatus.active)
                   .FirstOrDefault();

                if (session == null) return;


                var calculatedOrderResult = await GetCalculatedOrder(payload.ChargerId);
                var doesRefundNeed = calculatedOrderResult.RemainingTimeInMinutes >= 5;

                if (doesRefundNeed)
                    await UpdateOrder(calculatedOrderResult);

                string title = string.Empty;
                string body = string.Empty;

                if (payload.FinishReason == "REQUESTED_BY_CABLE_STATE")
                {
                    var updatedSession = await _dataContext.Sessions.FindAsync(session.Id);
                    updatedSession.Status = SessionStatus.WebhookCanceled;
                    updatedSession.ProviderStatus = ProviderChargingSessionStatus.closed;
                    updatedSession.EndDate = DateTime.Now;
                    updatedSession.UpdatedTime = DateTime.Now;
                    updatedSession.FinishReason = FinishReason.REQUESTED_BY_CABLE_STATE;

                    title = "Sessiya dayandırıldı";
                    body = "Kabel əlaqəsi 30 saniyə ərzində təmin edilmədiyi üçün sessiya dayandırıldı";
                }

                await _dataContext.OrderStatusChangedHooks.AddAsync(new OrderStatusChangedHook
                {
                    ChargerId = payload.ChargerId,
                    Connector = payload.Connector,
                    FinishReason = payload.FinishReason,
                    OrderUuid = payload.OrderUuid,
                    SessionId = session.Id,
                    Status = payload.Status,
                    CreatedDate = DateTime.Now
                });

                await _dataContext.SaveChangesAsync();

                Notification.PushNotification(new FirebasePayload { FCMToken = session.FCMToken, Title = title, Body = body });
                #region SignalR
                //var connectionId = ChargerHub.GetConnectionIdByOrderId(session.Order.OrderId);
                //if (!string.IsNullOrEmpty(connectionId))
                //{
                //    var hubUser = _hubContext.Clients.Clients(connectionId);

                //    var sessionStatusResponse = await _chargePointAction.GetSessionStatus(session.Order.OrderId);

                //    await hubUser.SendAsync("From_Server_To_Client", sessionStatusResponse);

                //};
                #endregion
            }
        }

        private async Task UpdateOrder(CurrentOrderCalculatedStatusResponse calculatedOrderResult)
        {
            var order = await _dataContext.Orders.FindAsync(calculatedOrderResult.OrderId);
            var refundProviderResponse = await _cibPayService.RefundSpecificAmount(new RefundSpecificAmoundOrderCommand { OrderId = calculatedOrderResult.ProviderOrderId, RefundAmount = calculatedOrderResult.RemainingAmount });

            var refundOrder = refundProviderResponse.Data.Orders.First();

            order.AmountRefunded = refundOrder.AmountRefunded;
            order.Updated = refundOrder.Updated;
            order.Description = refundOrder.Description;
            order.Status = PaymentStatus.Refunded;
            await _dataContext.SaveChangesAsync();
        }

        private async Task<CurrentOrderCalculatedStatusResponse> GetCalculatedOrder(string chargepointId)
        {
            var session = await _dataContext.Sessions
                .AsNoTracking()
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x => x.ChargerPointId == chargepointId && x.Status == SessionStatus.Charging && x.ProviderStatus == ProviderChargingSessionStatus.active);

            if (session == null) return null;

            var pricePerHour = session.PricePerHour;
            var startDate = session.StartDate;
            var duration = session.Duration;

            var usedTimeSpan = DateTime.Now - startDate.Value;
            var remainingTimeInMinutes = (int)(duration - usedTimeSpan).TotalMinutes;

            decimal spentAmount = (decimal)(remainingTimeInMinutes / 60.0) * pricePerHour;
            spentAmount = Math.Round(spentAmount, 2);

            return new CurrentOrderCalculatedStatusResponse
            {
                RemainingTimeInMinutes = remainingTimeInMinutes,
                TotalAmount = session.Order.AmountCharged,
                RemainingAmount = spentAmount,
                TotalTimeInMinutes = session.DurationInMinutes,
                SessionId = session.Id,
                OrderId = session.OrderId,
                ProviderOrderId = session.Order.OrderId,
            };
        }

        private class CurrentOrderCalculatedStatusResponse
        {
            public decimal TotalAmount { get; set; }
            public decimal RemainingAmount { get; set; }
            public int RemainingTimeInMinutes { get; set; }
            public double TotalTimeInMinutes { get; set; }
            public int OrderId { get; set; }
            public string SessionId { get; set; }
            public string ProviderOrderId { get; set; }
        }
    }
}

