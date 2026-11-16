namespace CinemaSystem.Pricing.Api.Strategies;

public class AdultPricingStrategy : ITicketPricingStrategy
{
    public decimal CalculatePrice(decimal basePrice, int quantity)
    {
        if (quantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        return basePrice * quantity;
    }
}