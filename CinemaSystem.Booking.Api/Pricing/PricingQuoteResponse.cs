namespace CinemaSystem.Booking.Api.Pricing;

public record PricingQuoteResponse(
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);