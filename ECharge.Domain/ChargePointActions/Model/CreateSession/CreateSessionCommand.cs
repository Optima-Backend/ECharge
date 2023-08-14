using System;
namespace ECharge.Domain.ChargePointActions.Model.CreateSession
{
    public class CreateSessionCommand
    {
        public DateTime PlannedStartDate { get; set; } = DateTime.Now;
        public DateTime PlannedEndDate { get; set; } = DateTime.Now.AddHours(2);
        public string ChargePointId { get; set; }
    }
}

