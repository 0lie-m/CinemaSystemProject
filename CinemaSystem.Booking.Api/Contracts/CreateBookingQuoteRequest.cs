namespace CinemaSystem.Booking.Api.Contracts;

public record CreateBookingQuoteRequest(
    int ShowingId,
    string? TicketType,
    int Quantity);
    