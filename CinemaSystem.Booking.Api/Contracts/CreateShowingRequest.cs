namespace CinemaSystem.Booking.Api.Contracts;

public record CreateShowingRequest(
    int MovieId,
    int ScreenId,
    DateTime StartTime,
    decimal BasePrice);
    