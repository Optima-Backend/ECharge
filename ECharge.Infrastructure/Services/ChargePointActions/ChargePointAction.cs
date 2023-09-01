using System.Net;
using ECharge.Domain.ChargePointActions.Interface;
using ECharge.Domain.ChargePointActions.Model.CreateSession;
using ECharge.Domain.ChargePointActions.Model.PaymentStatus;
using ECharge.Domain.CibPay.Interface;
using ECharge.Domain.CibPay.Model;
using ECharge.Domain.CibPay.Model.CreateOrder.Command;
using ECharge.Domain.Entities;
using ECharge.Domain.Enums;
using ECharge.Domain.EVtrip.Interfaces;
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
        private readonly string _cibPayBaseUrl;
        private readonly byte _durationForActivatingChargePoint;

        public ChargePointAction(ICibPayService cibPayService, DataContext context, IServiceProvider serviceProvider, IChargePointApiClient chargePointApiClient, IConfiguration configuration)
        {
            _cibPayService = cibPayService;
            _context = context;
            _serviceProvider = serviceProvider;
            _chargePointApiClient = chargePointApiClient;
            _durationForActivatingChargePoint = byte.Parse(configuration["SessionConfig:DurationForActivatingChargePoint"]);
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
            {
                return new { StatusCode = HttpStatusCode.BadRequest, Message = "There is not any charge point whith that ID" };
            }

            var currentSession = await _chargePointApiClient.GetChargingSessionsAsync(command.ChargePointId);

            if (currentSession is not null)
            {
                return new { StatusCode = HttpStatusCode.BadRequest, Message = "This charge point is currently busy" };
            }

            var totalMinutes = command.Duration.TotalMinutes;

            decimal amount = (decimal)(totalMinutes / 60.0) * command.Price;

            amount = Math.Round(amount, 2);

            var sessionId = Guid.NewGuid().ToString();

            var orderProviderResponse = await _cibPayService.CreateOrder(new CreateOrderCommand { Amount = amount, UserId = command.UserId, ChargePointId = command.ChargePointId, MerchantOrderId = sessionId, Name = command.Name, Email = command.Email });

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
                        MaxVoltage = singleChargePoint.Result.MaxVoltage
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

            if (providerResponse.StatusCode != HttpStatusCode.OK || !providerResponse.Data.Orders.Any())
            {
                return;
            }

            var providerOrder = providerResponse.Data.Orders.First();
            string status = providerOrder.Status;
            var paymentStatus = MapToPaymentStatus(status);

            var order = await _context.Orders
                .FirstOrDefaultAsync(x => x.OrderId == orderId);

            if (order != null)
            {
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
                    DateTimeOffset startTime = DateTimeOffset.Now.AddMinutes(_durationForActivatingChargePoint);
                    DateTimeOffset endTime = startTime.Add(session.Duration);

                    session.StartDate = startTime.DateTime;

                    await _context.SaveChangesAsync();

                    if (providerOrder.Status == "charged")
                    {
                        var factory = _serviceProvider.GetRequiredService<ISchedulerFactory>();
                        var scheduler = await factory.GetScheduler();

                        var scheduleJobs = new ScheduleJobs(scheduler);
                        await scheduleJobs.ScheduleJob(orderId, startTime, endTime);
                    }
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

        public async Task<object> GetSessionStatus(string orderId)
        {
            if (!await _context.Orders.AnyAsync(x => x.OrderId == orderId))
            {
                return new
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Session not found"
                };
            }

            var session = await _context.Sessions
                .Include(x => x.Order)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Order.OrderId == orderId);

            if (session.Status == SessionStatus.Charging)
            {
                var endTime = session.StartDate + session.Duration;

                var remaininChargingTime = endTime - DateTime.Now;
                var remaininChargingTimeInSeconds = remaininChargingTime.Value.TotalMilliseconds > 0 ? remaininChargingTime.Value.TotalSeconds : 0;

                return new
                {
                    StatusCode = HttpStatusCode.OK,
                    RemainingStartTimeInSeconds = 0,
                    RemainingChargingTimeInSeconds = (int)remaininChargingTimeInSeconds,
                    StartTime = session.StartDate,
                    EndTime = endTime,
                    DurationInSeconds = (int)session.Duration.TotalSeconds,
                    ChargingStatusCode = session.Status,
                    ChargingStatusDescription = "Charging",
                    Message = "Session is active. Vehicle is charging",
                    ChargePointId = session.ChargerPointId,
                    PricePerHour = session.PricePerHour,
                    MaxAmperage = session.MaxAmperage,
                    MaxVoltage = session.MaxVoltage,
                    Name = session.ChargePointName
                };
            }
            else if ((session.Status == SessionStatus.NotCharging || session.Status == SessionStatus.Complated) && session.Order.Status == PaymentStatus.Charged)
            {
                TimeSpan remainingStartTime = default;

                if (session.Status == SessionStatus.NotCharging)
                {
                    var paymentDateTime = session.StartDate.Value;
                    var diff = paymentDateTime - DateTime.Now;
                    remainingStartTime = diff.TotalMilliseconds > 0 ? diff : default;
                }

                var endTime = session.StartDate + session.Duration;
                TimeSpan remainingTime = default;
                string chargingStatus = session.Status == SessionStatus.NotCharging ? "NotCharging" : "Complated";
                string message = session.Status == SessionStatus.NotCharging ? "Session will start at start time" : "Session has complated";
                string message_2 = "Charge point is activating...";

                return new
                {
                    StatusCode = HttpStatusCode.OK,
                    RemainingStartTimeInSeconds = (int)remainingStartTime.TotalSeconds,
                    RemainingChargingTimeInSeconds = (int)remainingTime.TotalSeconds,
                    StartTime = session.StartDate,
                    EndTime = endTime,
                    DurationInSeconds = (int)session.Duration.TotalSeconds,
                    ChargingStatusCode = session.Status,
                    ChargingStatusDescription = chargingStatus,
                    Message = remainingStartTime.TotalMilliseconds >= 0 ? message : message_2,
                    ChargePointId = session.ChargerPointId,
                    PricePerHour = session.PricePerHour,
                    MaxAmperage = session.MaxAmperage,
                    MaxVoltage = session.MaxVoltage,
                    Name = session.ChargePointName
                };
            }
            else if (session.Order.Status != PaymentStatus.Charged && session.Order.Status == PaymentStatus.New)
            {
                return new
                {
                    StatusCode = HttpStatusCode.PaymentRequired,
                    ChargingStatusCode = 4,
                    Message = "This order has not been paid yet"
                };
            }
            else if (session.Status == SessionStatus.Canceled && session.Order.Status == PaymentStatus.Refunded)
            {
                return new
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ChargingStatusCode = 5,
                    Message = "An issue occurred while activating the charge point, and your money has been refunded."
                };
            }
            else if (session.Status == SessionStatus.Canceled && session.Order.Status == PaymentStatus.Charged)
            {
                return new
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    ChargingStatusCode = 6,

                    Message = "There was an issue during the activation of the Charge Point. Your money will be refunded shortly."
                };
            }

            return new
            {
                StatusCode = HttpStatusCode.InternalServerError,
                ChargingStatusCode = 6,
                Message = "Something went wrong"
            };

        }
    }
}
