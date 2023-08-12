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
    
    public int Duration { get; set; }
    
    public int PricePerHour { get; set; }
    
    //Foreign key for Transaction table
    public int TransactionId { get; set; }
    
    //Navigation property for Session
    public Transaction? Transaction { get; set; }
    
    [Column("Charging status")]
    public SessionStatus Status { get; set; }
    
    public DateTime UpdatedTime { get; set; }
    
}