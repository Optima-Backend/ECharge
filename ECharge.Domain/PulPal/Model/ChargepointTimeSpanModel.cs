using System;

namespace ECharge.Domain.PulPal.Model;

public class ChargepointTimeSpanModel
{
    public string? ChargepointId { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime StopDate { get; set; }
}