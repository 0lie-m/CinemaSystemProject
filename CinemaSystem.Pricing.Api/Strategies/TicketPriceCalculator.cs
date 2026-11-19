namespace CinemaSystem.Pricing.Api.Strategies;

public class TicketPriceCalculator
{
    private ITicketPricingStrategy strategy;

    public TicketPriceCalculator(ITicketPricingStrategy strategy)
    {
        this.strategy = strategy;
    }

    public void SetStrategy(ITicketPricingStrategy strategy)
    {
        this.strategy = strategy;
    }

    public decimal CalculatePrice(decimal basePrice, int quantity)
    {
        return strategy.CalculatePrice(basePrice, quantity);
    }
}