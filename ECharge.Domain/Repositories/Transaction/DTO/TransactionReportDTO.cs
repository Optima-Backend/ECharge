namespace ECharge.Domain.Repositories.Transaction.DTO;

public class TransactionReportDTO
{
    public int AllTransactionCount { get; set; }
    public int UnpaidTransactionCount { get; set; }
    public int RejectedTransactionCount { get; set; }
    public int ChargedTransactionCount { get; set; }
    public int RefundedTransactionCount { get; set; }
    public int DeclinedTransactionCount { get; set; }
    public decimal TotalTransactionAmount { get; set; }
    public decimal TotalChargedTransactionAmount { get; set; }
    public decimal TotalRefundedTransactionAmount { get; set; }
    public decimal TotalUnpaidTransactionAmount { get; set; }

    public decimal TotalDeclinedTransactionAmount { get; set; }
    public decimal TotalRejectedTransactionAmount { get; set; }
    
    public decimal TotalProfit { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}