using System.Net;
using ECharge.Domain.ChargePointActions.Interface;
using ECharge.Domain.ChargePointActions.Model.CreateSession;
using ECharge.Domain.CibPay.Interface;
using ECharge.Domain.CibPay.Model.CreateOrder.Command;
using ECharge.Domain.Entities;
using ECharge.Domain.Enums;
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
        private const string CibPayBaseUrl = "https://checkout-preprod.cibpay.co/pay/";

        public ChargePointAction(ICibPayService cibPayService, DataContext context, IServiceProvider serviceProvider)
        {
            _cibPayService = cibPayService;
            _context = context;
            _serviceProvider = serviceProvider;
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
            var serviceFee = 5;
            var totalMinutes = (int)(command.PlannedEndDate - command.PlannedStartDate).TotalMinutes;
            var amount = Math.Round(totalMinutes / 60.0, 2) * serviceFee;

            var orderProviderResponse = await _cibPayService.CreateOrder(new CreateOrderCommand { Amount = amount });

            if (orderProviderResponse.StatusCode == HttpStatusCode.Created && orderProviderResponse.Data.Orders.Any())
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var orderId = orderProviderResponse.Data.Orders.First().Id;

                    var transactionEntity = new Transaction
                    {
                        Link = CibPayBaseUrl + orderId,
                        OrderId = orderId,
                        CreatedDate = DateTime.Now,
                        Status = PaymentStatus.New
                    };

                    _context.Transactions.Add(transactionEntity);

                    var sessionEntity = new Session
                    {
                        ChargerPointId = command.ChargePointId,
                        Duration = totalMinutes,
                        StartDate = command.PlannedStartDate,
                        EndDate = command.PlannedEndDate,
                        PricePerHour = serviceFee,
                        Status = SessionStatus.NotCharging,
                        Transaction = transactionEntity
                    };
                    _context.Sessions.Add(sessionEntity);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new
                    {
                        StatusCode = HttpStatusCode.Created,
                        Amount = amount,
                        PaymentUrl = transactionEntity.Link,
                        OrderId = orderId,
                        ChargePointId = command.ChargePointId,
                        DurationMins = totalMinutes
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
                var transaction = await _context.Transactions.FirstOrDefaultAsync(x => x.OrderId == orderId);

                string status = providerResponse.Data.Orders.First().Status;

                var paymentStatus = MapToPaymentStatus(status);

                transaction.Status = paymentStatus;

                transaction.UpdatedDate = DateTime.Now;

                //_context.Transactions.Update(transaction);

                var session = _context.Sessions.FirstOrDefault(x => x.TransactionId == transaction.Id);

                var diff = transaction.UpdatedDate.Value - session.StartDate;

                session.EndDate += diff;
                session.StartDate += diff;

                await _context.SaveChangesAsync();


                if (providerResponse.Data.Orders.FirstOrDefault().Status == "charged")
                {
                    var factory = _serviceProvider.GetRequiredService<ISchedulerFactory>();
                    var scheduler = await factory.GetScheduler();

                    var scheduleJobs = new ScheduleJobs(scheduler);
                    await scheduleJobs.ScheduleJob(session.StartDate, session.EndDate, session.ChargerPointId);
                }
            }
        }

        public async Task<object> GetPaymentStatus(string orderId)
        {
            if (!await _context.Transactions.AnyAsync(x => x.OrderId == orderId))
            {
                return new
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Payment trasnaction not found"
                };
            }

            return _context.Transactions.FirstOrDefault(x => x.OrderId == orderId);
        }
    }
}
