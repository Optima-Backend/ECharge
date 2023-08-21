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
        private const string CibPayBaseUrl = "https://checkout-preprod.cibpay.co/pay/";

        public ChargePointAction(ICibPayService cibPayService, DataContext context, IServiceProvider serviceProvider, IChargePointApiClient chargePointApiClient)
        {
            _cibPayService = cibPayService;
            _context = context;
            _serviceProvider = serviceProvider;
            _chargePointApiClient = chargePointApiClient;
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
            var currentSession = await _chargePointApiClient.GetChargingSessionsAsync(command.ChargePointId);

            if (currentSession is not null)
            {
                return new { StatusCode = HttpStatusCode.BadRequest, Message = "This charge point is currently busy" };
            }

            var totalMinutes = (int)(command.PlannedEndDate - command.PlannedStartDate).TotalMinutes;
            var amount = Math.Round(totalMinutes / 60.0m, 2) * command.Price;
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
                        Updated = providerOrder.Updated
                    };

                    await _context.Orders.AddAsync(orderEntity);

                    var transactionEntity = new Transaction
                    {
                        Order = orderEntity,
                        CreatedDate = DateTime.Now,
                        Status = PaymentStatus.New
                    };

                    await _context.Transactions.AddAsync(transactionEntity);

                    var sessionEntity = new Session
                    {
                        Id = sessionId,
                        UserId = command.UserId,
                        Name = command.Name,
                        Email = command.Email,
                        ChargerPointId = command.ChargePointId,
                        Duration = totalMinutes,
                        StartDate = command.PlannedStartDate,
                        EndDate = command.PlannedEndDate,
                        PricePerHour = command.Price,
                        Status = SessionStatus.NotCharging,
                        Transaction = transactionEntity
                    };
                    await _context.Sessions.AddAsync(sessionEntity);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new
                    {
                        StatusCode = HttpStatusCode.Created,
                        Amount = amount,
                        PaymentUrl = CibPayBaseUrl + providerOrder.Id,
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

            if (providerResponse.StatusCode == HttpStatusCode.OK && providerResponse.Data.Orders.Any())
            {
                var transaction = await _context.Transactions.Include(x => x.Order).FirstOrDefaultAsync(x => x.Order.OrderId == orderId);

                var providerOrder = providerResponse.Data.Orders.First();
                string status = providerResponse.Data.Orders.First().Status;

                var paymentStatus = MapToPaymentStatus(status);

                transaction.Order.AmountCharged = providerOrder.AmountCharged;
                transaction.Order.AmountRefunded = providerOrder.AmountRefunded;
                transaction.Order.Status = paymentStatus;
                transaction.Order.Description = providerOrder.Description;
                transaction.Order.MerchantOrderId = providerOrder.MerchantOrderId;
                transaction.Order.Pan = providerOrder.Pan;
                transaction.UpdatedDate = providerOrder.Updated;
                transaction.Status = paymentStatus;

                var session = _context.Sessions.FirstOrDefault(x => x.TransactionId == transaction.Id);

                var duration = (int)(session.EndDate - session.StartDate).TotalMinutes;
                session.StartDate = DateTime.Now.AddMinutes(1);
                session.EndDate = session.StartDate.AddMinutes(duration + 1);
                session.Duration = (int)(session.EndDate - session.StartDate).TotalMinutes;

                _context.Sessions.Update(session);

                await _context.SaveChangesAsync();

                if (providerResponse.Data.Orders.FirstOrDefault().Status == "charged")
                {
                    var factory = _serviceProvider.GetRequiredService<ISchedulerFactory>();
                    var scheduler = await factory.GetScheduler();

                    var scheduleJobs = new ScheduleJobs(scheduler);
                    await scheduleJobs.ScheduleJob(session.StartDate, session.EndDate, session.ChargerPointId, session.Id);
                }
            }
        }

        public async Task<PaymentStatusResponse> GetPaymentStatus(string orderId)
        {
            if (!await _context.Transactions.Include(x => x.Order).AnyAsync(x => x.Order.OrderId == orderId))
            {
                return new PaymentStatusResponse
                {
                    Message = "Payment trasnaction not found",
                    StatusCode = HttpStatusCode.NotFound
                };
            }

            var session = await _context.Sessions.Include(x => x.Transaction).ThenInclude(x => x.Order).FirstOrDefaultAsync(x => x.Transaction.Order.OrderId == orderId);
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
            if (!await _context.Transactions.Include(x => x.Order).AnyAsync(x => x.Order.OrderId == orderId))
            {
                return new
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Session not found"
                };
            }

            var session = await _context.Sessions.Include(x => x.Transaction).ThenInclude(x => x.Order).FirstOrDefaultAsync(x => x.Transaction.Order.OrderId == orderId);

            if (session.Status == SessionStatus.Charging)
            {
                var duration = session.EndDate.AddSeconds(-10) - session.StartDate;
                var remainingTime = session.EndDate.AddSeconds(-10) - DateTime.Now;
                remainingTime = remainingTime.TotalMilliseconds > 0 ? remainingTime : default;

                return new
                {
                    StatusCode = HttpStatusCode.OK,
                    RemainingTime = remainingTime,
                    StartingTime = session.StartDate,
                    EndTime = session.EndDate.AddSeconds(-10),
                    Duration = duration,
                    ChargingStatus = "Charging",
                    Message = "Session is active. Vehicle is charging",
                };
            }
            else if (session.Status == SessionStatus.NotCharging || session.Status == SessionStatus.Complated)
            {
                var duration = session.EndDate.AddSeconds(-10) - session.StartDate;
                TimeSpan remainingTime = default;
                string chargingStatus = session.Status == SessionStatus.NotCharging ? "NotCharging" : "Complated";
                string message = session.Status == SessionStatus.NotCharging ? "Session will start at starting time" : "Session has complated";

                return new
                {
                    StatusCode = HttpStatusCode.OK,
                    RemainingTime = remainingTime,
                    StartingTime = session.StartDate,
                    EndTime = session.EndDate.AddSeconds(-10),
                    Duration = duration,
                    ChargingStatus = chargingStatus,
                    Message = message
                };
            }

            return new
            {
                StatusCode = HttpStatusCode.NotFound,
                Message = "Something went wrong"
            };

        }
    }
}
