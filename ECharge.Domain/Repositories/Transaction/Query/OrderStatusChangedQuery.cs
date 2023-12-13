namespace ECharge.Domain.Repositories.Transaction.Query;

public class OrderStatusChangedQuery
{
    public int PageSize { get; set; } = 10;
    public int PageIndex { get; set; } = 1;
    public string ChargePointId { get; set; }
    public string OrderUuid { get; set; }
    public DateTime? CreatedDateFrom { get; set; }
    public DateTime? CreatedDateTo { get; set; }
    public string SessionId { get; set; }
}