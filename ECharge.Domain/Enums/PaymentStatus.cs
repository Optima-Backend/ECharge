namespace ECharge.Domain.Enums;

public enum PaymentStatus
{
    New,
    Prepared,
    Authorized,
    Charged,
    Reversed,
    Refunded,
    Rejected,
    Fraud,
    Declined,
    Chargedback,
    Credited,
    Error
}