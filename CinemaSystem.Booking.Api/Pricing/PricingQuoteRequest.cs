namespace CinemaSystem.Booking.Api.Pricing;

public record PricingQuoteRequest(
    string TicketType,
    decimal BasePrice,
    int Quantity,
    bool IsPremiumMovie,
    decimal OccupancyRate);