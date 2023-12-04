using System.Net;
using ECharge.Domain.ChargePointActions.Interface;
using ECharge.Domain.ChargePointActions.Model.CreateSession;
using ECharge.Domain.ChargePointActions.Model.PaymentStatus;
using ECharge.Domain.CibPay.Interface;
using ECharge.Domain.CibPay.Model;
using ECharge.Domain.CibPay.Model.CreateOrder.Command;
using ECharge.Domain.DTOs;
using ECharge.Domain.Entities;
using ECharge.Domain.Enums;
using ECharge.Domain.EVtrip.Interfaces;
using ECharge.Domain.Job.Interface;
using ECharge.Infrastructure.Services.DatabaseContext;
using ECharge.Infrastructure.Services.Quartz;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace ECharge.Infrastructure.Services.ChargePointActions
{
    public class ChargePointAction : IChargePointAction
    {
        private readonly ICibPayService _cibPayService;
        private readonly DataContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly IChargePointApiClient _chargePointApiClient;
        private readonly IChargeSession _chargeSession;
        private readonly DataContext _dataContext;
        private readonly string _cibPayBaseUrl;
        private readonly byte _durationForActivatingChargePoint;

        public ChargePointAction(ICibPayService cibPayService, DataContext context, IServiceProvider serviceProvider,
            IChargePointApiClient chargePointApiClient, IConfiguration configuration, IChargeSession chargeSession,
            DataContext dataContext)
        {
            _cibPayService = cibPayService;
            _context = context;
            _serviceProvider = serviceProvider;
            _chargePointApiClient = chargePointApiClient;
            _chargeSession = chargeSession;
            _dataContext = dataContext;
            _durationForActivatingChargePoint = byte.Parse(configuration["SessionConfig:DurationForActivatingChargePoint"]!);
            _cibPayBaseUrl = configuration["CibPay:PaymentUrl"];
        }

        private PaymentStatus MapToPaymentStatus(string status)
        {
            return status.ToLower() switch
            {
                "charged" => PaymentStatus.Charged,
                "declined" => PaymentStatus.Declined,
                "new" => PaymentStatus.New,
                "rejected" => PaymentStatus.Rejected,
                "error" => PaymentStatus.Error,
                "refunded" => PaymentStatus.Refunded,
                "prepared" => PaymentStatus.Prepared,
                "authorized" => PaymentStatus.Authorized,
                "reversed" => PaymentStatus.Reversed,
                "fraud" => PaymentStatus.Fraud,
                "chargedback" => PaymentStatus.Chargedback,
                "credited" => PaymentStatus.Credited,
                _ => throw new ArgumentOutOfRangeException(nameof(status), $"Not expected status value: {status}")
            };
        }

        public async Task<object> GenerateLink(CreateSessionCommand command)
        {
            var singleChargePoint = await _chargePointApiClient.GetSingleChargerAsync(command.ChargePointId);

            if (!singleChargePoint.Success)
                return new { StatusCode = HttpStatusCode.BadRequest, Message = "There is not any charge point whith that ID" };

            if (singleChargePoint.Result.Status != "Available")
                return new { StatusCode = HttpStatusCode.BadRequest, Message = "Charge point not available" };

            var currentSession = await _chargePointApiClient.GetChargingSessionsAsync(command.ChargePointId);

            if (currentSession != null)
                return new { StatusCode = HttpStatusCode.BadRequest, Message = "This charge point is currently busy" };

            var totalMinutes = command.Duration.TotalMinutes;

            decimal amount = (decimal)(totalMinutes / 60.0) * command.Price;

            amount = Math.Round(amount, 2);

            var sessionId = Guid.NewGuid().ToString();

            var orderProviderResponse = await _cibPayService.CreateOrder(new CreateOrderCommand
            {
                Amount = amount,
                UserId = command.UserId,
                ChargePointId = command.ChargePointId,
                MerchantOrderId = sessionId,
                Name = command.Name,
                Email = command.Email
            });

            if (orderProviderResponse.StatusCode == HttpStatusCode.Created && orderProviderResponse.Data.Orders.Any())
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var providerOrder = orderProviderResponse.Data.Orders.First();

                    var orderEntity = new Order
                    {
                        OrderId = providerOrder.Id,
                        Amount = providerOrder.Amount,
                        AmountCharged = providerOrder.AmountCharged,
                        AmountRefunded = providerOrder.AmountRefunded,
                        Currency = providerOrder.Currency,
                        Description = providerOrder.Description,
                        Status = MapToPaymentStatus(providerOrder.Status),
                        MerchantOrderId = providerOrder.MerchantOrderId,
                        Pan = providerOrder.Pan,
                        Created = providerOrder.Created,
                        Updated = providerOrder.Updated,
                        OrderCreatedAt = DateTime.Now
                    };

                    await _context.Orders.AddAsync(orderEntity);

                    var sessionEntity = new Session
                    {
                        Id = sessionId,
                        UserId = command.UserId,
                        Name = command.Name,
                        Email = command.Email,
                        ChargerPointId = command.ChargePointId,
                        DurationInMinutes = totalMinutes,
                        Duration = command.Duration,
                        PricePerHour = command.Price,
                        Status = SessionStatus.NotCharging,
                        Order = orderEntity,
                        ChargePointName = singleChargePoint.Result.Name,
                        MaxAmperage = singleChargePoint.Result.MaxAmperage,
                        MaxVoltage = singleChargePoint.Result.MaxVoltage,
                        FCMToken = command.FCMToken
                    };
                    await _context.Sessions.AddAsync(sessionEntity);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new
                    {
                        StatusCode = HttpStatusCode.Created,
                        Amount = amount,
                        PaymentUrl = _cibPayBaseUrl + providerOrder.Id,
                        OrderId = providerOrder.Id,
                        command.ChargePointId,
                        DurationMins = totalMinutes,
                        Message = string.Empty
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new { StatusCode = HttpStatusCode.InternalServerError, Message = ex.Message };
                }
            }
            else
            {
                return new { StatusCode = HttpStatusCode.NotFound, Message = "Something went wrong on Creating Order" };
            }
        }

        public async Task PaymentHandler(string orderId)
        {
            var providerResponse = await _cibPayService.GetOrderInfo(orderId);

            if (providerResponse.StatusCode != HttpStatusCode.OK || !providerResponse.Data.Orders.Any()) return;

            var providerOrder = providerResponse.Data.Orders.First();
            string status = providerOrder.Status;
            var paymentStatus = MapToPaymentStatus(status);

            var order = await _context.Orders
                .FirstOrDefaultAsync(x => x.OrderId == orderId);

            if (order == null) return;

            order.AmountCharged = providerOrder.AmountCharged;
            order.AmountRefunded = providerOrder.AmountRefunded;
            order.Status = paymentStatus;
            order.Description = providerOrder.Description;
            order.MerchantOrderId = providerOrder.MerchantOrderId;
            order.Pan = providerOrder.Pan;
            order.Updated = providerOrder.Updated;
            order.Status = paymentStatus;

            var session = _context.Sessions.FirstOrDefault(x => x.OrderId == order.Id);

            if (session != null)
            {
                await _context.SaveChangesAsync();

                if (providerOrder.Status == "charged")
                {
                    var factory = _serviceProvider.GetRequiredService<ISchedulerFactory>();
                    var scheduler = await factory.GetScheduler("echarge_actions");

                    var scheduleJobs = new ScheduleJobs(scheduler);
                    await scheduleJobs.ScheduleJob(session.Id);
                }
            }
        }

        public async Task<PaymentStatusResponse> GetPaymentStatus(string orderId)
        {
            if (!await _context.Orders.AnyAsync(x => x.OrderId == orderId))
            {
                return new PaymentStatusResponse
                {
                    Message = "Payment trasnaction not found",
                    StatusCode = HttpStatusCode.NotFound
                };
            }

            var session = await _context.Sessions.Include(x => x.Order).AsNoTracking().FirstOrDefaultAsync(x => x.Order.OrderId == orderId);
            var providerResponse = await _cibPayService.GetOrderInfo(orderId);

            SingleOrderResponse orderResponse = default;

            if (providerResponse.StatusCode == HttpStatusCode.OK && providerResponse.Data.Orders.Any())
                orderResponse = providerResponse.Data.Orders.FirstOrDefault();

            return new PaymentStatusResponse
            {
                Session = session,
                StatusCode = HttpStatusCode.OK,
                Message = "Founded trasnaction",
                SingleOrderResponse = orderResponse
            };
        }

        public async Task<SessionStatusResponse> GetSessionStatus(string orderId)
        {
            if (!await _context.Orders.AnyAsync(x => x.OrderId == orderId))
            {
                return new SessionStatusResponse
                {
                    ChargingStatusCode = 2,
                    Message = "Sessiya tapılmadı",
                    HasProblem = true,
                };
            }

            var session = await _context.Sessions
                .AsNoTracking()
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x => x.Order.OrderId == orderId);

            var cableStateHook = await _context.CableStateHooks
                .AsNoTracking()
                .Where(x => x.SessionId == session.Id).Select(x => new CableStateHookDTO
                {
                    ChargePointId = x.ChargePointId,
                    CableState = x.CableState,
                    Connector = x.Connector,
                    SessionId = x.SessionId,
                    CreatedDate = x.CreatedDate
                }).OrderByDescending(x => x.CreatedDate).FirstOrDefaultAsync();

            var orderStatusChangedHooks =
                await _dataContext.OrderStatusChangedHooks.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.SessionId == session.Id);

            var currentSessionOnThisChargePoint = await _chargePointApiClient.GetChargingSessionsAsync(session.ChargerPointId);

            if (session.Status == SessionStatus.NotCharging && session.Order.Status == PaymentStatus.Charged)
            {
                var endTime = session.StartDate + session.Duration;

                return new SessionStatusResponse
                {
                    Timer = null,
                    StartTime = session.StartDate,
                    EndTime = endTime,
                    DurationInSeconds = (int)session.Duration.TotalSeconds,
                    ChargingStatusCode = 0,
                    ChargingStatusDescription = "Aktivasiya...",
                    Message = "Charge Box aktiv edilir, zəhmət olmasa biraz gözləyin",
                    ChargePointId = session.ChargerPointId,
                    PricePerHour = session.PricePerHour,
                    MaxAmperage = session.MaxAmperage,
                    MaxVoltage = 22,
                    HasProblem = false,
                    Name = session.ChargePointName,
                    CableStatus = session.Order.CableState,
                    LastCableState = cableStateHook ?? default
                };
            }

            if (session.Status == SessionStatus.Charging &&
                session.ProviderStatus == ProviderChargingSessionStatus.active &&
                currentSessionOnThisChargePoint != null && currentSessionOnThisChargePoint.Status ==
                ProviderChargingSessionStatus.active.ToString())
            {
                var message = string.Empty;
                string description = string.Empty;
                
                var endTime = session.StartDate + session.Duration;

                int? timer = null;

                bool hasProblem = true;

                if (session.Order.CableState.HasValue)
                {
                    if (session.Order.CableState == CableState.A)
                    {
                        var diff = cableStateHook.CreatedDate.AddMinutes(3) - DateTime.Now;
                        timer = (int?)diff.TotalMilliseconds > 0 ? (int?)diff.TotalSeconds : null;
                        message = "Kabel bağlantısını bərpa etmək üçün 3 dəqiqə vaxtınız var";
                        description = "Kabel bağlantı xətası";
                    }
                    else if (session.Order.CableState == CableState.B)
                    {
                        var diff = cableStateHook.CreatedDate.AddMinutes(2) - DateTime.Now;
                        timer = (int?)diff.TotalMilliseconds > 0 ? (int?)diff.TotalSeconds : null;
                        message = "Avtomobil ilə Charge Box arasında əlaqə kəsilib, 2 dəqiqə ərzində sessiya sonlandırılacaq";
                        description = "Əlaqə xətası";
                    }
                    else if (session.Order.CableState == CableState.E || session.Order.CableState == CableState.F || session.Order.CableState == CableState.D)
                    {
                        var diff = cableStateHook.CreatedDate.AddMinutes(1) - DateTime.Now;
                        timer = (int?)diff.TotalMilliseconds > 0 ? (int?)diff.TotalSeconds : null;
                        message = "Bilinməyən xəta, 1 dəqiqə ərzində sessiya sonlandırılacaq";
                        description = "Bilinməyən xəta";
                    }
                    else if (session.Order.CableState == CableState.C)
                    {
                        var diff = endTime - DateTime.Now;
                        timer = (int?)diff.Value.TotalMilliseconds > 0 ? (int?)diff.Value.TotalSeconds : null;
                        message = "Sessiya davam edir. Nəqliyyat vasitəsi şarj olur";
                        hasProblem = false;
                        description = "Şarj edilir";
                    }
                }
                else
                {
                    var diff = endTime - DateTime.Now;
                    timer = (int?)diff.Value.TotalMilliseconds > 0 ? (int?)diff.Value.TotalSeconds : null;
                    message = "Kabel ilə avtomobil arasında əlaqəni təmin edin";
                    description = "Əlaqə xətası";
                }

                return new SessionStatusResponse
                {
                    Timer = (int)timer,
                    StartTime = session.StartDate,
                    EndTime = endTime,
                    DurationInSeconds = (int)session.Duration.TotalSeconds,
                    ChargingStatusCode = 1,
                    ChargingStatusDescription = description,
                    Message = message,
                    ChargePointId = session.ChargerPointId,
                    PricePerHour = session.PricePerHour,
                    MaxAmperage = session.MaxAmperage,
                    MaxVoltage = 22,
                    Name = session.ChargePointName,
                    HasProblem = hasProblem,
                    CableStatus = session.Order.CableState,
                    LastCableState = cableStateHook ?? default
                };
            }

            if (session.Status == SessionStatus.Charging && session.ProviderStatus == ProviderChargingSessionStatus.active && currentSessionOnThisChargePoint == null)
            {
                var endTime = session.StartDate + session.Duration;

                return new SessionStatusResponse
                {
                    Timer = null,
                    StartTime = session.StartDate,
                    EndTime = endTime,
                    DurationInSeconds = (int)session.Duration.TotalSeconds,
                    ChargingStatusCode = 1,
                    ChargingStatusDescription = "Deaktivasiya",
                    Message = "Charge box deaktiv edilir, zəhmət olmasa biraz gözləyin",
                    ChargePointId = session.ChargerPointId,
                    PricePerHour = session.PricePerHour,
                    MaxAmperage = session.MaxAmperage,
                    MaxVoltage = 22,
                    Name = session.ChargePointName,
                    HasProblem = false,
                    CableStatus = session.Order.CableState,
                    LastCableState = cableStateHook ?? default
                };
            }

            if (session.Status == SessionStatus.Complated && session.Order.Status == PaymentStatus.Charged)
            {
                var endTime = session.StartDate + session.Duration;
                string message = "Avtomobilin şarj prosesi uğurla başa çatdı";

                return new SessionStatusResponse
                {
                    Timer = null,
                    StartTime = session.StartDate,
                    EndTime = endTime,
                    DurationInSeconds = (int)session.Duration.TotalSeconds,
                    ChargingStatusCode = 3,
                    ChargingStatusDescription = "Uğurlu əməliyyat",
                    Message = message,
                    ChargePointId = session.ChargerPointId,
                    PricePerHour = session.PricePerHour,
                    MaxAmperage = session.MaxAmperage,
                    MaxVoltage = 22,
                    Name = session.ChargePointName,
                    HasProblem = false,
                    CableStatus = session.Order.CableState,
                    LastCableState = cableStateHook ?? default
                };
            }

            if (session.Status == SessionStatus.WebhookCanceled)
            {
                var message = string.Empty;
                var description = string.Empty;

                if (!session.Order.CableState.HasValue && session.Order.CableState != CableState.C)
                {
                    message = "Kabel bağlantısı təmin edilmədiyi üçün sessiya bitirildi və qalıq balansınız geri qaytarıldı";
                    description = "Ləğv və geri ödəniş";
                }

                if (session.Order.CableState.HasValue)
                {
                    if (session.Order.Status == PaymentStatus.Charged)
                    {
                        if (session.Order.CableState == CableState.A)
                        {
                            message = "3 dəqiqədən çox kabel bağlantısı olmadığı üçün sessiya bitirildi və qalan şarj zamanı 5 dəqiqədən az olduğuna görə qalıq balansınız geri qaytarılmayacaq";
                        }
                        else if (session.Order.CableState == CableState.B)
                        {
                            message = "2 dəqiqədən çox avtomobil ilə Charge Box arasında əlaqə olmadığı üçün sessiya sonlandırıldı və qalan şarj zamanı 5 dəqiqədən az olduğuna görə qalıq balansınız geri qaytarılmayacaq";
                        }
                        else if (session.Order.CableState == CableState.E || session.Order.CableState == CableState.F || session.Order.CableState == CableState.D)
                        {
                            message = "Bilinməyən xəta baş verdiyi üçün sessiya sonlandırıldı və qalan şarj zamanı 5 dəqiqədən az olduğuna görə qalıq balansınız geri qaytarılmayacaq";
                        }

                        description = "Geri odəniş olmadan ləğv";
                    }
                    else if (session.Order.Status == PaymentStatus.Refunded)
                    {
                        if (session.Order.CableState == CableState.A)
                        {
                            message = "3 dəqiqədən çox kabel bağlantısı olmadığı üçün sessiya bitirildi və qalıq balansınız geri qaytarıldı";
                        }
                        else if (session.Order.CableState == CableState.B)
                        {
                            message = "2 dəqiqədən çox avtomobil ilə Charge Box arasında əlaqə olmadığı üçün sessiya sonlandırıldı və qalıq balansınız geri qaytarıldı";
                        }
                        else if (session.Order.CableState == CableState.E || session.Order.CableState == CableState.F || session.Order.CableState == CableState.D)
                        {
                            message = "Bilinməyən xəta baş verdiyi üçün sessiya sonlandırıldı və qalıq balansınız geri qaytarıldı";
                        }

                        description = "Ləğv və geri ödəniş";
                    }
                }

                return new SessionStatusResponse
                {
                    Timer = null,
                    StartTime = session.StartDate,
                    EndTime = session.EndDate,
                    DurationInSeconds = (int)session.Duration.TotalSeconds,
                    ChargingStatusCode = 3,
                    ChargingStatusDescription = description,
                    Message = message,
                    ChargePointId = session.ChargerPointId,
                    PricePerHour = session.PricePerHour,
                    MaxAmperage = session.MaxAmperage,
                    MaxVoltage = 22,
                    Name = session.Name,
                    HasProblem = true,
                    CableStatus = session.Order.CableState,
                    LastCableState = cableStateHook ?? default
                };
            }

            if (session.StoppedByClient && session.Status == SessionStatus.StopByClient)
            {
                var message = string.Empty;
                var description = string.Empty;

                if (session.Order.Status == PaymentStatus.Charged)
                {
                    description = "Geri odəniş olmadan ləğv";
                    message = "Sessiya admin tərəfindən dayandırıldı və qalan şarj zamanı 5 dəqiqədən az olduğuna görə qalıq balansınız geri qaytarılmayacaq.";
                }
                else if (session.Order.Status == PaymentStatus.Refunded)
                {
                    description = "Ləğv və geri ödəniş";
                    message = "Sessiya admin tərəfindən dayandırıldı və qalıq balansınız geri qaytarıldı.";
                }   
                
                return new SessionStatusResponse
                {
                    Timer = null,
                    StartTime = session.StartDate,
                    EndTime = session.EndDate,
                    DurationInSeconds = (int)session.Duration.TotalSeconds,
                    ChargingStatusCode = 3,
                    ChargingStatusDescription = description,
                    Message = message,
                    ChargePointId = session.ChargerPointId,
                    PricePerHour = session.PricePerHour,
                    MaxAmperage = session.MaxAmperage,
                    MaxVoltage = 22,
                    Name = session.Name,
                    HasProblem = true,
                    CableStatus = session.Order.CableState,
                    LastCableState = cableStateHook ?? default
                };
            }
            
            return new SessionStatusResponse
            {
                ChargingStatusCode = 4,
                ChargingStatusDescription = "Xəta",
                Message = "Daxili server xətası",
                LastCableState = cableStateHook ?? default,
                HasProblem = true,
            };
        }

        public async Task StopByClient(string orderId)
        {
            int maxRetryCount = 5;
            int attempt = 0;
            bool tryAgainForStopByClient = false;
            var session = await _dataContext.Sessions.Include(x => x.Order).Include(x => x.CableStateHooks)
                .FirstOrDefaultAsync(x => x.Order.OrderId == orderId);

            if (session.Status == SessionStatus.Charging)
            {
                var providerChargingSession = await _chargePointApiClient.GetChargingSessionsAsync(session.ChargerPointId);
                if (providerChargingSession != null)
                {
                    while (attempt < maxRetryCount)
                    {
                        try
                        {
                            attempt++;

                            var result = await _chargeSession.Execute(session.Order.OrderId, false, false,
                                false, false, tryAgainForStopByClient ,true);

                            if (result is ChargeRequestStatus.StopSuccess)
                            {
                                break;
                            }

                            if (attempt == 1)
                                tryAgainForStopByClient = true;

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }

                        await Task.Delay(1000);
                    }
                }
            }
        }
    }
}