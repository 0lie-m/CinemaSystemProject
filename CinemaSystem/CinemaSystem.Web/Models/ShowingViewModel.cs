namespace CinemaSystem.Web.Models;

public class ShowingViewModel
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int ScreenId { get; set; }
    public string ScreenName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal BasePrice { get; set; }
    public int Capacity { get; set; }
    public int SeatsSold { get; set; }
    public int AvailableSeats { get; set; }
}