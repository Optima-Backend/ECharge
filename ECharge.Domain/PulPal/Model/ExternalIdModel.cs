using System.ComponentModel.DataAnnotations;

namespace ECharge.Domain.PulPal.Model;

public class ExternalIdModel
{
    [Required]
    public string? Link { get; set; }
    
    public string? Message { get; set; }
    
    public bool Success { get; set; }
}