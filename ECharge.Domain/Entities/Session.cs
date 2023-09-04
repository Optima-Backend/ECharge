using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ECharge.Domain.Enums;

namespace ECharge.Domain.Entities;

[Table("ChargePointSession")]
public class Session
{
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

    public int? MaxAmperage { get; set; }

}