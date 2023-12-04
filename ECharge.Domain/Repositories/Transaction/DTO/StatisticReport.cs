using ECharge.Domain.CibPay.Model;

namespace ECharge.Domain.Repositories.Transaction.DTO;

public class StatisticReport
{
    public string ChargePointId { get; set; }
    public string ChargePointName { get; set; }
    public long TotalDurationInMinutes { get; set; }
    public decimal TotalProfit { get; set; }
    public int TotalTransactions { get; set; }
    
}