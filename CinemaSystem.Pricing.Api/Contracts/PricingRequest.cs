namespace CinemaSystem.Pricing.Api.Contracts;

// Keep the API request contract in a named namespace.
public record PricingRequest(
    string? TicketType,
    decimal BasePrice,
    int Quantity,
    bool IsPremiumMovie,
    decimal OccupancyRate);