namespace CinemaSystem.Booking.Api.Models;

public class BookedTicket
{
    public Guid Id { get; set; }

    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public string TicketType { get; set; } = string.Empty;
    public decimal PricePaid { get; set; }
}