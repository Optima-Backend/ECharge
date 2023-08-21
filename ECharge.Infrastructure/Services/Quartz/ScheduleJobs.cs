using ECharge.Domain.Job.Interface;
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

        public async Task ScheduleJob(DateTime startDate, DateTime endDate, string chargePointId, string sessionId)
        {
            var diff = endDate.AddSeconds(-10) - startDate;

            IJobDetail job = JobBuilder.Create<CustomJob>()
                .WithIdentity("customJob", "group1")
                .UsingJobData("chargePointId", chargePointId)
                .UsingJobData("sessionId", sessionId)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("customTrigger", "group1")
                .StartAt(new DateTimeOffset(startDate))
                .EndAt(new DateTimeOffset(endDate))
                .WithSimpleSchedule(x => x
                    .WithInterval(diff)
                    .RepeatForever())
                .Build();

            await _scheduler.ScheduleJob(job, trigger);
        }

    }

    public class CustomJob : IJob
    {
        private readonly IChargeSession _chargeSession;

        public CustomJob(IChargeSession chargeSession)
        {
            _chargeSession = chargeSession;
        }

        public Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            string chargePointId = dataMap.GetString("chargePointId");
            string sessionId = dataMap.GetString("sessionId");

            _chargeSession.Execute(chargePointId);

            return Task.CompletedTask;
        }
    }


}

