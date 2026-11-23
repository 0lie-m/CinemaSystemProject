namespace CinemaSystem.Pricing.Api.Contracts;

public record PricingResponse(
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);