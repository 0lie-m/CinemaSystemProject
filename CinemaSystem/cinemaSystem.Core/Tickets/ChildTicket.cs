namespace CinemaSystem.Core.Tickets;

public class ChildTicket : Ticket
{
    public ChildTicket(int showingId, decimal pricePaid)
        : base(showingId, pricePaid)
    {
    }
}