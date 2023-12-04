namespace ECharge.Domain.Repositories.Transaction.DTO;

public class TransactionReport
{
    public int ReportTransactionCount { get; set; }
    public decimal TotalChargedAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalRefundedAmount { get; set; }
    public decimal TotalProfit { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}