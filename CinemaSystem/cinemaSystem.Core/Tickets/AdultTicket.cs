namespace CinemaSystem.Core.Tickets;

public class AdultTicket : Ticket
{
    public AdultTicket(int showingId, decimal pricePaid)
        : base(showingId, pricePaid)
    {
    }
}