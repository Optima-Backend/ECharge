using ECharge.Domain.CibPay.Interface;
using ECharge.Domain.CibPay.Model.RefundOrder.Command;
using ECharge.Domain.Enums;
using ECharge.Domain.EVtrip.Interfaces;
using ECharge.Domain.Job.Interface;
using ECharge.Infrastructure.Services.DatabaseContext;
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

        public async Task ScheduleJob(string chargePointId, string orderId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var startJobName = $"Start Job Name: {orderId}";
            var stopJobName = $"Stop Job Name: {orderId}";
            var startTriggerName = $"Start Trigger Name: {orderId}";
            var stopTriggerName = $"Stop Trigger Name: {orderId}";

            IJobDetail startJob = JobBuilder.Create<CustomJob>()
                .WithIdentity(startJobName, "ECharge")
                .UsingJobData("orderId", orderId)
                .UsingJobData("chargePointId", chargePointId)
                .UsingJobData("jobName", startJobName)
                .Build();

            IJobDetail endJob = JobBuilder.Create<CustomJob>()
                .WithIdentity(stopJobName, "ECharge")
                .UsingJobData("orderId", orderId)
                .UsingJobData("chargePointId", chargePointId)
                .UsingJobData("jobName", stopJobName)
                .Build();

            ITrigger startTrigger = TriggerBuilder.Create()
                .WithIdentity(startTriggerName, "ECharge")
                .StartAt(startTime)
                .Build();

            ITrigger stopTrigger = TriggerBuilder.Create()
                .WithIdentity(stopTriggerName, "ECharge")
                .StartAt(endTime)
                .Build();

            await _scheduler.ScheduleJob(startJob, startTrigger);
            await _scheduler.ScheduleJob(endJob, stopTrigger);
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
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            string orderId = dataMap.GetString("orderId");
            string chargePointId = dataMap.GetString("chargePointId");
            string jobName = dataMap.GetString("jobName");
            int maxRetryCount = 5;
            int attempt = 0;
            bool success = false;
            bool tryAgain = false;
            bool webhookStopped = false;

            if (jobName.Contains("Stop"))
            {
                var providerChargingSession = await _chargePointApiClient.GetChargingSessionsAsync(chargePointId);
                if (providerChargingSession == null)
                    webhookStopped = true;

            }

            //var sessionStatus = await _dataContext.OrderStatusChangedHooks.AsNoTracking().Include(x => x.Session).ThenInclude(x => x.Order).AnyAsync(x => x.Session.Order.OrderId == orderId && x.Session.Status == SessionStatus.WebhookCanceled && x.Session.ProviderStatus == ProviderChargingSessionStatus.closed);

            if (jobName.Contains("Start") || (jobName.Contains("Stop") && !webhookStopped))
            {
                while (attempt < maxRetryCount)
                {
                    try
                    {
                        attempt++;

                        var result = await _chargeSession.Execute(orderId, tryAgain);

                        if (result == ChargeRequestStatus.StartSuccess || result == ChargeRequestStatus.StopSuccess)
                        {
                            success = true;
                            break;
                        }

                        if (attempt == 1)
                            tryAgain = true;

                    }
                    catch { }

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

}