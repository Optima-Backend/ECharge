using System.ComponentModel.DataAnnotations;

namespace ECharge.Domain.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SessionId { get; set; }

        public string UserId { get; set; }

        public string Message { get; set; }

        public string Title { get; set; }

        public bool HasSeen { get; set; }

        public bool IsCableStatus { get; set; }

        [Required]
        public string FCMToken { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public Session Session { get; set; }
    }
}

