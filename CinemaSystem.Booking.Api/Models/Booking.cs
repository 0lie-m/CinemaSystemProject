namespace CinemaSystem.Booking.Api.Models;

public class Booking
{
    public int Id { get; set; }

    public int ShowingId { get; set; }
    public Showing Showing { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public decimal TotalPrice { get; set; }

    public List<BookedTicket> Tickets { get; set; } = new();
}