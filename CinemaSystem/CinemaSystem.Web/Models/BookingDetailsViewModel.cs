namespace CinemaSystem.Web.Models;

public class BookingDetailsViewModel
{
    public int Id { get; set; }
    public int ShowingId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string ScreenName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public decimal TotalPrice { get; set; }
    public int Quantity { get; set; }
    public List<BookedTicketViewModel> Tickets { get; set; } = new();
}

public class BookedTicketViewModel
{
    public Guid Id { get; set; }
    public string TicketType { get; set; } = string.Empty;
    public decimal PricePaid { get; set; }
}