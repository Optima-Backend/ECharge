using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ECharge.Domain.Enums;

namespace ECharge.Domain.Entities;

[Table("PaymentTransaction")]
public class Transaction
{
    [Key]
    public int Id { get; set; }

    [Column("PaymentStatus")]
    [Required]
    public PaymentStatus Status { get; set; }

    public decimal AmountRefund { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; }
}