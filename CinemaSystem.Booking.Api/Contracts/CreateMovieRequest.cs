namespace CinemaSystem.Booking.Api.Contracts;

public record CreateMovieRequest(
    string? Title,
    int DurationMinutes,
    bool IsPremium);