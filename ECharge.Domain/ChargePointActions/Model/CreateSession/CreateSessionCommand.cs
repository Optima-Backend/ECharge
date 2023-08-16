using System;
namespace ECharge.Domain.ChargePointActions.Model.CreateSession
{
    public class CreateSessionCommand
    {
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public string ChargePointId { get; set; }
    }
}

