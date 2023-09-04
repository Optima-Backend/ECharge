namespace ECharge.Infrastructure.Services.PaymentCalculator;

public static class PaymentCalculatorHandler
{
    
    public static decimal Calculate(decimal totalPrice, double totalMinutes, int roundDigits)
    {
        decimal amount = (decimal)(totalMinutes / 60.0) * totalPrice;

        return Math.Round(amount,roundDigits);
    }
    
    public static decimal Calculate(decimal totalPrice, double totalMinutes)
    {
        decimal amount = (decimal)(totalMinutes / 60.0) * totalPrice;

        return Math.Round(amount,2);
    }
    
}
