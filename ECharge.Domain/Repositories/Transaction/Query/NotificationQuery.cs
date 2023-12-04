using System.ComponentModel.DataAnnotations;

namespace ECharge.Domain.Repositories.Transaction.Query
{
    public class NotificationQuery
    {
        public int PageSize { get; set; } = 10;
        public int PageIndex { get; set; } = 1;

        [Required]
        public string UserId { get; set; }

        public string SessionId { get; set; }

        public string Message { get; set; }

        public string Title { get; set; }

        public bool? HasSeen { get; set; }

        public bool? IsCableStatus { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}

