namespace CinemaSystem.Booking.Api.Contracts;

public record CreateBookingRequest(
    int ShowingId,
    string? TicketType,
    int Quantity,
    decimal? ExpectedTotalPrice);