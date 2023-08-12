using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ECharge.Domain.Enums;

namespace ECharge.Domain.Entities;

[Table("PaymentTransaction")]
public class Transaction
{
    public int Id { get; set; }
    
    public required string Link { get; set; }

    [Column("PaymentStatus")]
    [Required]
    public PaymentStatus Status { get; set; }
    
    public required string OrderId { get; set; }
    
    //Navigation property
    public Session? Session { get; set; }
    
    public int AmountRefund { get; set; }
    
    [Required]
    public DateTime CreatedDate { get; set; }
    
    public DateTime UpdatedDate { get; set; }
    


}