using CinemaSystem.Core.Tickets;

namespace CinemaSystem.Core.Creators;

public class ChildTicketCreator : TicketCreator
{
    protected override Ticket CreateTicket(
        int showingId, decimal pricePaid)
    {
        return new ChildTicket(showingId, pricePaid);
    }
}