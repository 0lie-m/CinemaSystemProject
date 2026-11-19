namespace CinemaSystem.Booking.Api.Contracts;

public record CreateScreenRequest(
    string? Name,
    int Capacity);