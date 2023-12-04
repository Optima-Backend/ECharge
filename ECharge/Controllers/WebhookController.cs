using ECharge.Domain.CibPay.Interface;
using ECharge.Domain.CibPay.Model.RefundOrder.Command;
using ECharge.Domain.DTOs;
using ECharge.Domain.Entities;
using ECharge.Domain.Enums;
using ECharge.Domain.EVtrip.Models;
using ECharge.Infrastructure.Services.DatabaseContext;
using ECharge.Infrastructure.Services.FirebaseNotification;
using ECharge.Infrastructure.Services.Quartz;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace ECharge.Api.Controllers
{
    [Route("charger")]
    public class WebhookController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly ICibPayService _cibPayService;
        private readonly IServiceProvider _serviceProvider;

        public WebhookController(DataContext dataContext, ICibPayService cibPayService, IServiceProvider serviceProvider)
        {
            _dataContext = dataContext;
            _cibPayService = cibPayService;
            _serviceProvider = serviceProvider;
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

                if (session != null)
                {
                    var orderId = session.Order.OrderId;

                    var updatedOrder = await _dataContext.Orders.FindAsync(session.OrderId);

                    string title = string.Empty;
                    string body = string.Empty;

                    if (payload.CableState == "A")
                    {
                        updatedOrder.CableState = CableState.A;
                        title = "Kabel xətası";
                        body = "Kabel bağlantısını bərpa etmək üçün 3 dəqiqə vaxtınız var əks təqdirdə sessiya sonlandırılacaq";

                    }
                    else if (payload.CableState == "B")
                    {
                        updatedOrder.CableState = CableState.B;

                        title = "Əlaqə xətası";
                        body = "Avtomobil ilə Charge Box arasında əlaqə kəsilib, 2 dəqiqə ərzində sessiya sonlandırılacaq";

                        var factory = _serviceProvider.GetRequiredService<ISchedulerFactory>();
                        var scheduler = await factory.GetScheduler("echarge_actions");

                        var scheduleJobs = new ScheduleJobs(scheduler);
                        await scheduleJobs.ScheduleJob2(session.Id);

                    }
                    else if (payload.CableState == "C")
                    {
                        updatedOrder.CableState = CableState.C;
                        title = "Şarj davam edir";
                        body = "Əlaqəsi yenidən təmin edildiyi üçün sessiya davam edir";
                    }
                    else if (payload.CableState == "E" || payload.CableState == "F" || payload.CableState == "D")
                    {
                        if (payload.CableState == "E")
                            updatedOrder.CableState = CableState.E;
                        else if (payload.CableState == "F")
                            updatedOrder.CableState = CableState.F;
                        else if (payload.CableState == "D")
                            updatedOrder.CableState = CableState.D;

                        title = "Bilinməyən xəta";
                        body = "1 dəqiqə ərzində sessiya sonlandırılacaq";
                    }

                    await _dataContext.Notifications.AddAsync(new Notification
                    {
                        UserId = session.UserId,
                        FCMToken = session.FCMToken,
                        SessionId = session.Id,
                        Title = title,
                        Message = body,
                        IsCableStatus = true
                    });

                    FirebaseNotification.PushNotification(new FirebasePayload { FCMToken = session.FCMToken, Title = title, Body = body });
                }

                await _dataContext.CableStateHooks.AddAsync(new CableStateHook
                {
                    CableState = payload.CableState,
                    ChargePointId = payload.ChargerId,
                    Connector = payload.Connector,
                    CreatedDate = DateTime.Now,
                    SessionId = session != null ? session.Id : null
                });


                await _dataContext.SaveChangesAsync();

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

        [HttpPost("/order/status-changed")]
        public async Task HandleOrderStatusChanged([FromBody] OrderStatusChangedPayload payload)
        {
            if (payload != null)
            {
                if (await _dataContext.Sessions.AnyAsync(x =>
                        x.ChargerPointId == payload.ChargerId && x.Status == SessionStatus.Charging &&
                        x.ProviderStatus == ProviderChargingSessionStatus.active))
                {
                    var session = _dataContext.Sessions
                        .Include(x => x.Order)
                        .FirstOrDefault(x =>
                            x.ChargerPointId == payload.ChargerId && x.Status == SessionStatus.Charging &&
                            x.ProviderStatus == ProviderChargingSessionStatus.active);

                    var cableStateHook = await _dataContext.CableStateHooks
                    .AsNoTracking()
                    .Where(x => x.SessionId == session.Id).Select(x => new CableStateHookDTO
                    {
                        ChargePointId = x.ChargePointId,
                        CableState = x.CableState,
                        Connector = x.Connector,
                        SessionId = x.SessionId,
                        CreatedDate = x.CreatedDate
                    }).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();

                    var calculatedOrderResult = await GetCalculatedOrder(payload.ChargerId);
                    var doesRefundNeed = calculatedOrderResult.RemainingTimeInMinutes >= 5;

                    string title = string.Empty;
                    string body = string.Empty;

                    if (payload.FinishReason == "REQUESTED_BY_CLIENT" && session!.StoppedByClient)
                        session.Status = SessionStatus.StopByClient;
                    else
                        session!.Status = SessionStatus.WebhookCanceled;

                    session.ProviderStatus = ProviderChargingSessionStatus.closed;
                    session.EndDate = DateTime.Now;
                    session.UpdatedTime = DateTime.Now;

                    Dictionary<string, FinishReason> finishReasonMapping = new()
                    {
                        { FinishReason.REQUESTED_BY_CABLE_STATE.ToString(), FinishReason.REQUESTED_BY_CABLE_STATE },
                        { FinishReason.CHARGING_LOW_POWER.ToString(), FinishReason.CHARGING_LOW_POWER },
                        { FinishReason.PLUG_AND_CHARGE_STOP.ToString(), FinishReason.PLUG_AND_CHARGE_STOP },
                        { FinishReason.REQUESTED_BY_CLIENT.ToString(), FinishReason.REQUESTED_BY_CLIENT },
                        { FinishReason.REQUESTED_BY_CPO.ToString(), FinishReason.REQUESTED_BY_CPO },
                        { FinishReason.REQUESTED_BY_OWNER.ToString(), FinishReason.REQUESTED_BY_OWNER },
                        { FinishReason.CHARGING_START_FAIL.ToString(), FinishReason.CHARGING_START_FAIL },
                        { FinishReason.CHARGER_ALARM.ToString(), FinishReason.CHARGER_ALARM }
                        
                    };

                    if (finishReasonMapping.TryGetValue(payload.FinishReason, out var value))
                        session.FinishReason = value;

                    if (cableStateHook != null && cableStateHook.CableState != CableState.C.ToString() && session.FinishReason != FinishReason.REQUESTED_BY_CLIENT)
                    {
                        if (cableStateHook.CableState == CableState.A.ToString())
                        {
                            if (doesRefundNeed)
                            {
                                await UpdateOrder(calculatedOrderResult);
                                title = "Ləğv və geri ödəniş";
                                body = "3 dəqiqədən çox kabel bağlantısı bərpa edilmədiyi üçün sessiya bitirildi və qalıq balansınız geri qaytarıldı.";
                            }
                            else
                            {
                                title = "Geri odəniş olmadan ləğv";
                                body = "3 dəqiqədən çox kabel bağlantısı bərpa edilmədiyi üçün sessiya bitirildi və qalan şarj zamanı 5 dəqiqədən az olduğuna görə qalıq balansınız geri qaytarılmayacaq.";
                            }

                        }
                        else if (cableStateHook.CableState == CableState.B.ToString())
                        {
                            if (doesRefundNeed)
                            {
                                await UpdateOrder(calculatedOrderResult);
                                title = "Ləğv və geri ödəniş";
                                body = "2 dəqiqədən çox avtomobil ilə Charge Box arasında əlaqə olmadığı üçün sessiya sonlandırıldı və qalıq balansınız geri qaytarıldı.";
                            }
                            else
                            {
                                title = "Geri odəniş olmadan ləğv";
                                body = "2 dəqiqədən çox avtomobil ilə Charge Box arasında əlaqə olmadığı üçün sessiya sonlandırıldı və qalan şarj zamanı 5 dəqiqədən az olduğuna görə qalıq balansınız geri qaytarılmayacaq.";
                            }
                        }
                        else if (cableStateHook.CableState == CableState.D.ToString() || cableStateHook.CableState == CableState.E.ToString() || cableStateHook.CableState == CableState.F.ToString())
                        {
                            if (doesRefundNeed)
                            {
                                await UpdateOrder(calculatedOrderResult);
                                title = "Ləğv və geri ödəniş";
                                body = "Bilinməyən xəta baş verdiyi üçün sessiya sonlandırıldı və qalıq balansınız geri qaytarıldı.";
                            }
                            else
                            {
                                title = "Geri odəniş olmadan ləğv";
                                body = "Bilinməyən xəta baş verdiyi üçün sessiya sonlandırıldı və qalan şarj zamanı 5 dəqiqədən az olduğuna görə qalıq balansınız geri qaytarılmayacaq.";
                            }
                        }
                    }
                    else if (cableStateHook == null && payload.FinishReason == FinishReason.REQUESTED_BY_CABLE_STATE.ToString())
                    {
                        await UpdateOrder(calculatedOrderResult);
                        title = "Ləğv və geri ödəniş";
                        body = "Kabel bağlantısı təmin edilmədiyi üçün sessiya bitirildi və qalıq balansınız geri qaytarıldı.";
                    }

                    if (session.FinishReason == FinishReason.REQUESTED_BY_CLIENT && session.StoppedByClient)
                    {
                        session.Status = SessionStatus.StopByClient;
                        
                        if (doesRefundNeed)
                        {
                            await UpdateOrder(calculatedOrderResult);
                            title = "Ləğv və geri ödəniş";
                            body = "Sessiya admin tərəfindən dayandırıldı və qalıq balansınız geri qaytarıldı.";
                        }
                        else
                        {
                            title = "Geri odəniş olmadan ləğv";
                            body = "Sessiya admin tərəfindən dayandırıldı və qalan şarj zamanı 5 dəqiqədən az olduğuna görə qalıq balansınız geri qaytarılmayacaq.";
                        }
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

                    await _dataContext.Notifications.AddAsync(new Notification
                    {
                        UserId = session.UserId,
                        FCMToken = session.FCMToken,
                        SessionId = session.Id,
                        Title = title,
                        Message = body,
                        IsCableStatus = false
                    });

                    await _dataContext.SaveChangesAsync();

                    FirebaseNotification.PushNotification(new FirebasePayload { FCMToken = session.FCMToken, Title = title, Body = body });
                }
                else
                {
                    await _dataContext.OrderStatusChangedHooks.AddAsync(new OrderStatusChangedHook
                    {
                        ChargerId = payload.ChargerId,
                        Connector = payload.Connector,
                        FinishReason = payload.FinishReason,
                        OrderUuid = payload.OrderUuid,
                        Status = payload.Status,
                        CreatedDate = DateTime.Now
                    });
                    await _dataContext.SaveChangesAsync();
                }
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

