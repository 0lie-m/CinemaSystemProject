namespace CinemaSystem.Pricing.Api.Strategies;

public class StudentPricingStrategy : ITicketPricingStrategy
{
    public decimal CalculatePrice(decimal basePrice, int quantity)
    {
        return basePrice * 0.8m * quantity;
    }
}