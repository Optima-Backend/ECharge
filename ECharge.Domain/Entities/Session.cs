using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ECharge.Domain.Enums;

namespace ECharge.Domain.Entities;

[Table("ChargePointSession")]
public class Session
{
    public int Id { get; set; }

    [Required]
    public required string ChargerPointId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public int Duration { get; set; }

    [Required]
    public int PricePerHour { get; set; }

    public int TransactionId { get; set; }

    public Transaction Transaction { get; set; }

    [Column("Charging status")]
    public SessionStatus Status { get; set; }

    public DateTime? UpdatedTime { get; set; }

}