using ECharge.Domain.CibPay.Interface;
using ECharge.Domain.CibPay.Model.RefundOrder.Command;
using ECharge.Domain.Entities;
using ECharge.Domain.Enums;
using ECharge.Domain.EVtrip.Interfaces;
using ECharge.Domain.Job.Interface;
using ECharge.Infrastructure.Services.DatabaseContext;
using ECharge.Infrastructure.Services.FirebaseNotification;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace ECharge.Infrastructure.Services.Quartz
{
    public class ScheduleJobs
    {
        private readonly IScheduler _scheduler;

        public ScheduleJobs(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public async Task ScheduleJob(string sessionId)
        {
            using (var _dataContext = new DataContext())
            {
                var session = await _dataContext.Sessions.Include(x => x.Order).FirstOrDefaultAsync(x => x.Id == sessionId);

                if (session != null)
                {
                    var startTime = DateTimeOffset.Now;
                    var endTime = startTime.Add(session.Duration);

                    session.StartDate = startTime.DateTime;
                    var startJobName = $"Start Job Name: {session.Order.OrderId}";
                    var stopJobName = $"Stop Job Name: {session.Order.OrderId}";
                    var startTriggerName = $"Start Trigger Name: {session.Order.OrderId}";
                    var stopTriggerName = $"Stop Trigger Name: {session.Order.OrderId}";

                    var startJob = JobBuilder.Create<CustomJob>()
                        .WithIdentity(startJobName, "ECharge")
                    .UsingJobData("orderId", session.Order.OrderId)
                        .UsingJobData("chargePointId", session.ChargerPointId)
                        .UsingJobData("jobName", startJobName)
                        .Build();

                    var endJob = JobBuilder.Create<CustomJob>()
                        .WithIdentity(stopJobName, "ECharge")
                        .UsingJobData("orderId", session.Order.OrderId)
                        .UsingJobData("chargePointId", session.ChargerPointId)
                        .UsingJobData("jobName", stopJobName)
                        .Build();

                    var startTrigger = TriggerBuilder.Create()
                        .WithIdentity(startTriggerName, "ECharge")
                        .StartAt(DateTimeOffset.Now.AddSeconds(5))
                        .Build();

                    var stopTrigger = TriggerBuilder.Create()
                        .WithIdentity(stopTriggerName, "ECharge")
                        .StartAt(endTime.AddSeconds(5))
                        .Build();

                    await _scheduler.ScheduleJob(startJob, startTrigger);
                    await _scheduler.ScheduleJob(endJob, stopTrigger);
                    await _dataContext.SaveChangesAsync();
                }
            }
        }

        public async Task ScheduleJob2(string sessionId)
        {
            var stopJobName = $"End Job Name: {sessionId}";
            var stopTriggerName = $"End Trigger Name: {sessionId}";

            var endJob = JobBuilder.Create<WebhookJob>()
                .WithIdentity(stopJobName, "ECharge")
                .UsingJobData("sessionId", sessionId)
                .UsingJobData("jobName", stopJobName)
                .Build();

            var endTrigger = TriggerBuilder.Create()
                .WithIdentity(stopTriggerName, "ECharge")
                .StartAt(DateTimeOffset.Now.AddMinutes(2))
                .Build();

            await _scheduler.ScheduleJob(endJob, endTrigger);
        }
    }

    public class CustomJob : IJob
    {
        private readonly IChargeSession _chargeSession;
        private readonly ICibPayService _cibPayService;
        private readonly DataContext _dataContext;
        private readonly IChargePointApiClient _chargePointApiClient;

        public CustomJob(IChargeSession chargeSession, ICibPayService cibPayService, DataContext dataContext, IChargePointApiClient chargePointApiClient)
        {
            _chargeSession = chargeSession;
            _cibPayService = cibPayService;
            _dataContext = dataContext;
            _chargePointApiClient = chargePointApiClient;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;

            string orderId = dataMap.GetString("orderId");
            string chargePointId = dataMap.GetString("chargePointId");
            string jobName = dataMap.GetString("jobName");
            int maxRetryCount = 5;
            int attempt = 0;
            bool success = false;
            bool tryAgainForStart = false;
            bool tryAgainForStop = false;
            bool webhookStopped = false;

            if (jobName != null && jobName.Contains("Stop"))
            {
                var providerChargingSession = await _chargePointApiClient.GetChargingSessionsAsync(chargePointId);
                if (providerChargingSession == null)
                    webhookStopped = true;

            }

            if (jobName != null && (jobName.Contains("Start") || (jobName.Contains("Stop") && !webhookStopped)))
            {
                while (attempt < maxRetryCount)
                {
                    try
                    {
                        attempt++;

                        var result = await _chargeSession.Execute(orderId, tryAgainForStart, tryAgainForStop, false,
                            false, false,false);

                        if (result is ChargeRequestStatus.StartSuccess or ChargeRequestStatus.StopSuccess)
                        {
                            success = true;
                            break;
                        }

                        if (attempt == 1)
                        {
                            if (jobName.Contains("Start"))
                            {
                                tryAgainForStart = true;
                            }
                            else if (jobName.Contains("Stop"))
                            {
                                tryAgainForStop = true;
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    await Task.Delay(1000);
                }

                if (!success && attempt == 5)
                {
                    var startJobKey = new JobKey($"Start Job Name: {orderId}");
                    var stopJobKey = new JobKey($"Stop Job Name: {orderId}");

                    if (await context.Scheduler.CheckExists(startJobKey))
                        await context.Scheduler.DeleteJob(startJobKey);

                    if (await context.Scheduler.CheckExists(stopJobKey))
                        await context.Scheduler.DeleteJob(stopJobKey);

                    await UpdateOrder(orderId);
                }
            }
        }

        private async Task UpdateOrder(string orderId)
        {
            var refundProviderResponse = await _cibPayService.RefundOrder(new RefundOrderCommand { OrderId = orderId });
            var refundOrder = refundProviderResponse.Data.Orders.First();

            var order = await _dataContext.Orders.FirstOrDefaultAsync(x => x.OrderId == orderId);

            order.AmountRefunded = refundOrder.AmountRefunded;
            order.Updated = refundOrder.Updated;
            order.Description = refundOrder.Description;
            order.Status = PaymentStatus.Refunded;

            await _dataContext.SaveChangesAsync();
        }
    }

    public class WebhookJob : IJob
    {
        private readonly IChargeSession _chargeSession;
        private readonly ICibPayService _cibPayService;
        private readonly DataContext _dataContext;
        private readonly IChargePointApiClient _chargePointApiClient;

        public WebhookJob(IChargeSession chargeSession, ICibPayService cibPayService, DataContext dataContext, IChargePointApiClient chargePointApiClient)
        {
            _chargeSession = chargeSession;
            _cibPayService = cibPayService;
            _dataContext = dataContext;
            _chargePointApiClient = chargePointApiClient;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            string sessionId = dataMap.GetString("sessionId");
            int maxRetryCount = 5;
            int attempt = 0;
            bool success = false;
            bool tryAgainForWebHookStatusB = false;
            var session = await _dataContext.Sessions.Include(x => x.Order).Include(x => x.CableStateHooks).FirstOrDefaultAsync(x => x.Id == sessionId);

            if (session.CableStateHooks.OrderByDescending(x => x.CreatedDate).FirstOrDefault().CableState == "B" && session.Status == SessionStatus.Charging)
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
                                tryAgainForWebHookStatusB, true, false, false);

                            if (result is ChargeRequestStatus.StartSuccess or ChargeRequestStatus.StopSuccess)
                            {
                                var calculatedOrderResult = await GetCalculatedOrder(session.Id);
                                var title = string.Empty;
                                var body = string.Empty;
                                if (calculatedOrderResult.RemainingTimeInMinutes >= 5)
                                {
                                    await UpdateOrder(calculatedOrderResult);
                                    title = "Ləğv və geri ödəniş";
                                    body = "5 dəqiqədən çox avtomobil ilə Charge Box arasında əlaqə olmadığı üçün sessiya sonlandırıldı və qalıq balansınız geri qaytarıldı.";
                                }
                                else
                                {
                                    title = "Geri odəniş olmadan ləğv";
                                    body = "5 dəqiqədən çox avtomobil ilə Charge Box arasında əlaqə olmadığı üçün sessiya sonlandırıldı və qalan şarj zamanı 5 dəqiqədən az olduğuna görə qalıq balansınız geri qaytarılmayacaq.";
                                }


                                await _dataContext.Notifications.AddAsync(new Notification
                                {
                                    UserId = session.UserId,
                                    FCMToken = session.FCMToken,
                                    SessionId = session.Id,
                                    Title = title,
                                    Message = body,
                                    IsCableStatus = false
                                });

                                FirebaseNotification.FirebaseNotification.PushNotification(new FirebasePayload { FCMToken = session.FCMToken, Title = title, Body = body });

                                success = true;

                                break;
                            }

                            if (attempt == 1)
                                tryAgainForWebHookStatusB = true;

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }

                        await Task.Delay(1000);
                    }

                    var stopJobKey = new JobKey($"Stop Job Name: {session.Order.OrderId}");

                    if (await context.Scheduler.CheckExists(stopJobKey))
                        await context.Scheduler.DeleteJob(stopJobKey);
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

        private async Task<CurrentOrderCalculatedStatusResponse> GetCalculatedOrder(string sessionId)
        {
            var session = await _dataContext.Sessions
                .AsNoTracking()
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x => x.Id == sessionId);

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