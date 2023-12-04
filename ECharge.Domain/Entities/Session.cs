using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ECharge.Domain.Enums;

namespace ECharge.Domain.Entities;

[Table("ChargePointSession")]
public class Session
{
    public Session()
    {
        CableStateHooks = new HashSet<CableStateHook>();
        Notifications = new HashSet<Notification>();
    }

    [Key]
    public string Id { get; set; }

    [Required]
    public string ChargerPointId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    public double DurationInMinutes { get; set; }

    [Required]
    public TimeSpan Duration { get; set; }

    [Required]
    public decimal PricePerHour { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; }

    [Column("Charging status")]
    public SessionStatus Status { get; set; }

    public DateTime? UpdatedTime { get; set; }

    public string UserId { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    public string ChargePointName { get; set; }

    public int? MaxVoltage { get; set; }

    public bool StoppedByClient { get; set; }

    [Required]
    public string FCMToken { get; set; }

    public int? MaxAmperage { get; set; }

    public double? EnergyConsumption { get; set; }

    public string ProviderSessionId { get; set; }

    public FinishReason? FinishReason { get; set; }

    public ProviderChargingSessionStatus ProviderStatus { get; set; }

    public virtual ICollection<CableStateHook> CableStateHooks { get; set; }
    public virtual ICollection<Notification> Notifications { get; set; }

}