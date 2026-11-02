namespace CinemaSystem.Pricing.Api.Strategies;
public interface ITicketPricingStrategy
{
    decimal CalculatePrice(decimal basePrice, int quantity);
}