using CinemaSystem.Pricing.Api.Strategies;

namespace CinemaSystem.Pricing.Api.Services;

public class DynamicPricingService
{
    public decimal CalculatePrice(
        ITicketPricingStrategy strategy,
        decimal basePrice,
        int quantity,
        bool isPremiumMovie,
        decimal occupancyRate = 0m)
    {
        if (basePrice < 0)
{
    throw new ArgumentOutOfRangeException(nameof(basePrice));
}

if (occupancyRate < 0m || occupancyRate > 1m)
{
    throw new ArgumentOutOfRangeException(nameof(occupancyRate));
}
        decimal adjustedPrice = basePrice;

        if (isPremiumMovie)
        {
            adjustedPrice += 2m;
        }

        if (occupancyRate >= 0.8m)
        {
            adjustedPrice *= 1.1m;
        }

        var calculator = new TicketPriceCalculator(strategy);

        if (quantity < 0)
{
    throw new ArgumentOutOfRangeException(nameof(quantity));
}

decimal unitPrice = calculator.CalculatePrice(adjustedPrice, 1);

decimal roundedUnitPrice = Math.Round(
    unitPrice, 2, MidpointRounding.AwayFromZero);

return roundedUnitPrice * quantity;
    }
}