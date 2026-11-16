namespace CinemaSystem.Pricing.Api.Strategies;

public class ChildPricingStrategy : ITicketPricingStrategy
{
    public decimal CalculatePrice(decimal basePrice, int quantity)
    {
        return basePrice * 0.7m * quantity;
    }
}