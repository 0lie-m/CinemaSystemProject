namespace CinemaSystem.Pricing.Api.Strategies;

public class AdultPricingStrategy : ITicketPricingStrategy
{
    public decimal CalculatePrice(decimal basePrice, int quantity)
    {
        // Reject negative quantities using the standard argument guard.
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);

        return basePrice * quantity;
    }
}