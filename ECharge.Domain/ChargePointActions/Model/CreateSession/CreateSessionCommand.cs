
using System.ComponentModel.DataAnnotations;

namespace ECharge.Domain.ChargePointActions.Model.CreateSession
{
    public class CreateSessionCommand
    {
        [Required]
        public required TimeSpan Duration { get; set; }
        [Required]
        public required string ChargePointId { get; set; }
        public required string UserId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        [Required]
        public required decimal Price { get; set; }
        [Required]
        public string FCMToken { get; set; }
    }
}

