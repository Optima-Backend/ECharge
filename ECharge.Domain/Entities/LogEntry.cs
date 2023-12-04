using System;
namespace ECharge.Domain.Entities
{
    public class LogEntry
    {
        public int Id { get; set; }
        public string RequestResponseDetails { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

