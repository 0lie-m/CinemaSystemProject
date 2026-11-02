namespace CinemaSystem.Pricing.Api.Strategies;
public class AdultPricingStrategy : ITicketPricingStrategy
{
    public decimal CalculatePrice(decimal basePrice, int quantity)
    {
        return basePrice * quantity;
    }
}