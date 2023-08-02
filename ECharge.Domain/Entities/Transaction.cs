using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECharge.Domain.Entities;

[Table("PulpalTransactions")]
public class Transaction
{
    public int Id { get; set; }
    
    [Required]
    public string? ExternalId { get; set; }
    
    [Column("ExternalIdCreationTime")]
    [Required]
    public DateTime CreatedDate { get; set; }
    
    public DateTime PaymentDate { get; set; }
    
    [DefaultValue(false)]
    public bool Status { get; set; }
    
    public string? Message { get; set; }
    
    public int StatusCode { get; set; }

}