namespace ECharge.Domain.Repositories.Transaction.DTO;

public class StatisticReportDTO
{
    public string ChargePointId { get; set; }
    public string ChargePointName { get; set; }
    public int TotalDurationInMinutes { get; set; }
    public int TotalProfit { get; set; }
    public int TotalTransactions { get; set; }
    public AdminTransactionDTO Transactions { get; set; }
    
}